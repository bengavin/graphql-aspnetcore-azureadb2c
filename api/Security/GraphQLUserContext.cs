using System.Security.Claims;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

namespace StarWars.API.Security;

public class GraphQLUserContextBuilder : IUserContextBuilder
{
    private readonly ITokenAcquisition _tokenProvider;

    public GraphQLUserContextBuilder(ITokenAcquisition tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public async ValueTask<IDictionary<string, object?>?> BuildUserContextAsync(HttpContext httpContext, object? wsPayload)
    {
        if (!httpContext.User.HasAnyScopeClaim(
            ClientApiScope.CharacterRead, 
            ClientApiScope.CharacterWrite, 
            ApiScope.CharacterWrite, 
            ApiScope.CharacterRead))
        {
            // Now, look to see if there is Bearer token auth available
            var authResult = await httpContext.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (authResult.Succeeded)
            {
                httpContext.User = authResult.Principal;
            }
            else 
            {
                // we need to obtain an access token with appropriate scopes (this -will- normally fail, but try anyway)
                var accessToken = await _tokenProvider.GetAccessTokenForUserAsync(new [] {
                    ClientApiScope.CharacterRead,
                    ClientApiScope.CharacterWrite
                });

                // Now, update the current context user to be based on this new token
                var newUser = GetBearerTokenPrincipal(accessToken, httpContext.User);
                httpContext.User = newUser;
            }
        }

        return new GraphQLUserContext(httpContext.User);
    }

    private ClaimsPrincipal GetBearerTokenPrincipal(string token, ClaimsPrincipal claimsPrincipal)
    {
        if (string.IsNullOrWhiteSpace(token)) { return claimsPrincipal; }

        var tokenPrincipal = token.ParseToken();
        return claimsPrincipal == null || claimsPrincipal.Identity == null || tokenPrincipal.Identity == null
             ? tokenPrincipal
             : new ClaimsPrincipal(new []
               { 
                    (ClaimsIdentity)tokenPrincipal.Identity,
                    (ClaimsIdentity)claimsPrincipal.Identity
               });
    }
}

public class GraphQLUserContext : Dictionary<string, object?>
{
    public GraphQLUserContext(ClaimsPrincipal user)
    {
        User = user;
    }

    public ClaimsPrincipal User { get; }
}
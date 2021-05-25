using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GraphQL.Authorization;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;

namespace StarWars.API.Security
{
    public class GraphQLUserContextBuilder : IUserContextBuilder
    {
        private readonly ITokenAcquisition _tokenProvider;

        public GraphQLUserContextBuilder(ITokenAcquisition tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public async Task<IDictionary<string, object>> BuildUserContext(HttpContext httpContext)
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

            var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var scopeClaims = AddScopeClaims(parsedToken);
            return new ClaimsPrincipal(new[] { new ClaimsIdentity(parsedToken.Claims.Concat(scopeClaims)), (ClaimsIdentity)claimsPrincipal.Identity });
        }

        private IEnumerable<Claim> AddScopeClaims(JwtSecurityToken parsedToken)
        {
            var retVal = new List<Claim>();
            var scopeClaims = parsedToken.Claims.Where(c => string.Equals(c.Type, "scp", StringComparison.OrdinalIgnoreCase));
            foreach (var claim in scopeClaims)
            {
                retVal.AddRange(claim.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(cv => new Claim(ClaimTypes.Role, cv)));
                retVal.Add(new Claim(ClaimConstants.Scope, claim.Value));
            }

            return retVal;
        }
    }

    public class GraphQLUserContext : Dictionary<string, object>, IProvideClaimsPrincipal
    {
        public GraphQLUserContext(ClaimsPrincipal user)
        {
            User = user;
        }

        public ClaimsPrincipal User { get; }
    }
}
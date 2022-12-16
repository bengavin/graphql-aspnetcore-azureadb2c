using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace StarWars.API.Security;

public static class SecurityExtensions
{
    private static string[] ScopeClaimTypes = new string[] { ClaimConstants.Scope, ClaimConstants.Scp };

    public static bool HasAnyScopeClaim(this ClaimsPrincipal user, params string[] scopes)
    {
        var scopeClaims = user.FindAll(c => ScopeClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase));
        var claimedScopes = scopeClaims.SelectMany(c => c.Value?.Split(new[] { ' ' }) ?? new string[0])
                                        .ToList();

        return scopes.Any(s => claimedScopes.Contains(s, StringComparer.OrdinalIgnoreCase));
    }

    public static AuthorizationPolicyBuilder RequireScopeClaim(this AuthorizationPolicyBuilder builder, params string[] scopes) =>
        builder.RequireAssertion(context => context.User.HasAnyScopeClaim(scopes));

    public static ClaimsPrincipal ParseToken(this string authorizationHeaderValue)
    {
        if (authorizationHeaderValue.StartsWith("Bearer "))
        {
            authorizationHeaderValue = authorizationHeaderValue.Substring("Bearer ".Length);
        }

        var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(authorizationHeaderValue);
        var scopeClaims = parsedToken.GetScopeClaims();
        return new ClaimsPrincipal(new[] { new ClaimsIdentity(parsedToken.Claims.Concat(scopeClaims)) });
    }

    public static IEnumerable<Claim> GetScopeClaims(this JwtSecurityToken parsedToken)
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
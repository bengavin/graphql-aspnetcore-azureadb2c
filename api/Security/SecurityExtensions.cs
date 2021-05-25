using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

namespace StarWars.API.Security
{
    public static class SecurityExtensions
    {
        private static string[] ScopeClaimTypes = new string[] { ClaimConstants.Scope, ClaimConstants.Scp };

        public static bool HasAnyScopeClaim(this ClaimsPrincipal user, params string[] scopes)
        {
            var scopeClaims = user.FindAll(c => ScopeClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase));
            var claimedScopes = scopeClaims.SelectMany(c => c.Value?.Split(new[] { ' ' }))
                                           .ToList();

            return scopes.Any(s => claimedScopes.Contains(s, StringComparer.OrdinalIgnoreCase));
        }

        public static AuthorizationPolicyBuilder RequireScopeClaim(this AuthorizationPolicyBuilder builder, params string[] scopes) =>
            builder.RequireAssertion(context => context.User.HasAnyScopeClaim(scopes));
    }
}
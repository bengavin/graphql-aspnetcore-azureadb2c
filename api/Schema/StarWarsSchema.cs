using GraphQL;
using GraphQL.Utilities;
using Microsoft.Extensions.DependencyInjection;
using StarWars.API.Security;
using System;

namespace StarWars.API.Schema
{
    public class StarWarsSchema : GraphQL.Types.Schema
    {
        public StarWarsSchema(IServiceProvider provider)
            : base(provider)
        {
            this.AuthorizeWith(Policies.AuthorizedUser);
            
            Query = provider.GetRequiredService<StarWarsQuery>();
            Mutation = provider.GetRequiredService<StarWarsMutation>();
        }
    }
}

using GraphQL;

namespace StarWars.API.Schema;

public class StarWarsSchema : GraphQL.Types.Schema
{
    public StarWarsSchema(IServiceProvider provider)
        : base(provider)
    {
        this.Authorize();
        
        Query = provider.GetRequiredService<StarWarsQuery>();
        Mutation = provider.GetRequiredService<StarWarsMutation>();
    }
}

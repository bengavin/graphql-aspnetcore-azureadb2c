namespace StarWars.API.Schema;

public class StarWarsSchema : GraphQL.Types.Schema
{
    public StarWarsSchema(IServiceProvider provider)
        : base(provider)
    {
        Query = provider.GetRequiredService<StarWarsQuery>();
        Mutation = provider.GetRequiredService<StarWarsMutation>();
    }
}

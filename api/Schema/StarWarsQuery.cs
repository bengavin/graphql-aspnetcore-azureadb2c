using GraphQL;
using GraphQL.Types;
using StarWars.API.Security;
using StarWars.API.Services;

namespace StarWars.API.Schema;

public class StarWarsQuery : ObjectGraphType
{
    public StarWarsQuery(IStarWarsDataService data)
    {
        Name = "Query";

        Field<CharacterInterface>("hero")
            .ResolveAsync(async context => await data.GetDroidByIdAsync("3"))
            .AuthorizeWithPolicy(Policies.CharacterAccess);

        Field<HumanType>("human")
            .Argument<NonNullGraphType<StringGraphType>>("id", "id of the human")
            .ResolveAsync(async context => await data.GetHumanByIdAsync(context.GetArgument<string>("id")))
            .AuthorizeWithPolicy(Policies.CharacterAccess);

        Field<DroidType>("droid")
            .Argument<NonNullGraphType<StringGraphType>>("id", "id of the droid")
            .ResolveAsync(async context => await data.GetDroidByIdAsync(context.GetArgument<string>("id")))
            .AuthorizeWithPolicy(Policies.CharacterAccess);
    }
}

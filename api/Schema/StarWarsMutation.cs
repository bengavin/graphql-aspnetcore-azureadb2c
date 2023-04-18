using GraphQL;
using GraphQL.Types;
using StarWars.API.Models;
using StarWars.API.Security;
using StarWars.API.Services;

namespace StarWars.API.Schema;

/// <example>
/// This is an example JSON request for a mutation
/// {
///   "query": "mutation ($human:HumanInput!){ createHuman(human: $human) { id name } }",
///   "variables": {
///     "human": {
///       "name": "Boba Fett"
///     }
///   }
/// }
/// </example>
public class StarWarsMutation : ObjectGraphType
{
    public StarWarsMutation(IStarWarsDataService data)
    {
        Name = "Mutation";

        Field<HumanType>("createHuman")
            .Argument<NonNullGraphType<HumanInputType>>("human")
            .Resolve(context =>
            {
                var human = context.GetArgument<Human>("human");
                return data.AddHuman(human);
            })
            .AuthorizeWithPolicy(Policies.CharacterWriteAccess);

        Field<HumanType>("updateHuman")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .Argument<NonNullGraphType<HumanInputType>>("human")
            .Resolve(context =>
            {
                var id = context.GetArgument<string>("id");
                var human = context.GetArgument<Human>("human");
                return data.UpdateHuman(id, human);
            })
            .AuthorizeWithPolicy(Policies.CharacterWriteAccess);

        Field<DroidType>("createDroid")
            .Argument<NonNullGraphType<DroidInputType>>("droid")
            .Resolve(context =>
            {
                var droid = context.GetArgument<Droid>("droid");
                return data.AddDroid(droid);
            })
            .AuthorizeWithPolicy(Policies.CharacterWriteAccess);

        Field<DroidType>("updateDroid")
            .Argument<NonNullGraphType<IdGraphType>>("id")
            .Argument<NonNullGraphType<DroidInputType>>("droid")
            .Resolve(context =>
            {
                var id = context.GetArgument<string>("id");
                var droid = context.GetArgument<Droid>("droid");
                return data.UpdateDroid(id, droid);
            })
            .AuthorizeWithPolicy(Policies.CharacterWriteAccess);
    }
}

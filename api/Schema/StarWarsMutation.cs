using GraphQL;
using GraphQL.Types;
using StarWars.API.Models;
using StarWars.API.Security;
using StarWars.API.Services;

namespace StarWars.API.Schema
{
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

            Field<HumanType>(
                "createHuman",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<HumanInputType>> { Name = "human" }
                ),
                resolve: context =>
                {
                    var human = context.GetArgument<Human>("human");
                    return data.AddHuman(human);
                }).AuthorizeWith(Policies.CharacterWriteAccess);

            Field<HumanType>(
                "updateHuman",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" },
                    new QueryArgument<NonNullGraphType<HumanInputType>> { Name = "human" }
                ),
                resolve: context =>
                {
                    var id = context.GetArgument<string>("id");
                    var human = context.GetArgument<Human>("human");
                    return data.UpdateHuman(id, human);
                }).AuthorizeWith(Policies.CharacterWriteAccess);

            Field<DroidType>(
                "createDroid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DroidInputType>> { Name = "droid" }
                ),
                resolve: context =>
                {
                    var droid = context.GetArgument<Droid>("droid");
                    return data.AddDroid(droid);
                }).AuthorizeWith(Policies.CharacterWriteAccess);

            Field<DroidType>(
                "updateDroid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id" },
                    new QueryArgument<NonNullGraphType<DroidInputType>> { Name = "droid" }
                ),
                resolve: context =>
                {
                    var id = context.GetArgument<string>("id");
                    var droid = context.GetArgument<Droid>("droid");
                    return data.UpdateDroid(id, droid);
                }).AuthorizeWith(Policies.CharacterWriteAccess);
        }
    }
}

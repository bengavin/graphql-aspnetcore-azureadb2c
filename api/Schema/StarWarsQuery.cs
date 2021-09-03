using System;
using GraphQL;
using GraphQL.Types;
using StarWars.API.Security;
using StarWars.API.Services;

namespace StarWars.API.Schema
{
    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery(IStarWarsDataService data)
        {
            Name = "Query";

            Field<CharacterInterface>("hero", resolve: context => data.GetDroidByIdAsync("3"))
                .AuthorizeWith(Policies.CharacterAccess);

            Field<ListGraphType<CharacterInterface>>("characters", 
                resolve: context => data.GetCharactersAsync())
                .AuthorizeWith(Policies.CharacterAccess);

            Field<HumanType>(
                "human",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id", Description = "id of the human" }
                ),
                resolve: context => data.GetHumanByIdAsync(context.GetArgument<string>("id"))
            ).AuthorizeWith(Policies.CharacterAccess);

            Func<IResolveFieldContext, string, object> func = (context, id) => data.GetDroidByIdAsync(id);

            FieldDelegate<DroidType>(
                "droid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "id", Description = "id of the droid" }
                ),
                resolve: func
            ).AuthorizeWith(Policies.CharacterAccess);
        }
    }
}

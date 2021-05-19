using GraphQL.Types;
using StarWars.API.Models;
using StarWars.API.Services;

namespace StarWars.API.Schema
{
    public class HumanType : ObjectGraphType<Human>
    {
        public HumanType(IStarWarsDataService data)
        {
            Name = "Human";

            Field(h => h.Id).Description("The id of the human.");
            Field(h => h.Name, nullable: true).Description("The name of the human.");

            Field<ListGraphType<CharacterInterface>>(
                "friends",
                resolve: context => data.GetFriends(context.Source)
            );
            Field<ListGraphType<EpisodeEnum>>("appearsIn", "Which movie they appear in.");

            Field(h => h.HomePlanet, nullable: true).Description("The home planet of the human.");

            Interface<CharacterInterface>();
        }
    }
}

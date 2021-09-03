using GraphQL.Types;
using StarWars.API.Models;

namespace StarWars.API.Schema
{
    public class HumanInputType : InputObjectGraphType<Human>
    {
        public HumanInputType()
        {
            Name = "HumanInput";
            Field(x => x.Name);
            Field<AlignmentEnum>("alignment", "The alignment of the character");
            Field<ListGraphType<EpisodeEnum>>("appearsIn", "The episodes this character appears in");
            Field<ListGraphType<IdGraphType>>("friends", "The id(s) of the friends of this character");
            Field(x => x.HomePlanet, nullable: true);
        }
    }
}

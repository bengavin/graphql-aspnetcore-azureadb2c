using GraphQL.Types;
using StarWars.API.Models;

namespace StarWars.API.Schema;

public class DroidInputType : InputObjectGraphType<Droid>
{
    public DroidInputType()
    {
        Name = "DroidInput";
        Field(x => x.Name);
        Field<AlignmentEnum>("alignment").Description("The alignment of the character");
        Field<ListGraphType<EpisodeEnum>>("appearsIn").Description("The episodes this character appears in");
        Field<ListGraphType<IdGraphType>>("friends").Description("The id(s) of the friends of this character");
        Field(x => x.PrimaryFunction, nullable: true);
    }
}

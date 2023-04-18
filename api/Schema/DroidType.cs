using GraphQL.Types;
using StarWars.API.Models;
using StarWars.API.Services;

namespace StarWars.API.Schema;

public class DroidType : ObjectGraphType<Droid>
{
    public DroidType(IStarWarsDataService data)
    {
        Name = "Droid";
        Description = "A mechanical creature in the Star Wars universe.";

        Field(d => d.Id).Description("The id of the droid.");
        Field(d => d.Name, nullable: true).Description("The name of the droid.");
        Field<AlignmentEnum>("alignment").Description("The alignment of the character")
                                         .Resolve(ctx => ctx.Source.Alignment);

        Field<ListGraphType<CharacterInterface>>("friends").Resolve(context => data.GetFriends(context.Source));

        Field<ListGraphType<EpisodeEnum>>("appearsIn").Description("Which movie they appear in.");
        Field(d => d.PrimaryFunction, nullable: true).Description("The primary function of the droid.");

        Interface<CharacterInterface>();
    }
}

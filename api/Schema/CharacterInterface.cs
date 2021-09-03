using GraphQL.Types;
using StarWars.API.Models;

namespace StarWars.API.Schema
{
    public class CharacterInterface : InterfaceGraphType<StarWarsCharacter>
    {
        public CharacterInterface()
        {
            Name = "Character";

            Field(d => d.Id).Description("The id of the character.");
            Field(d => d.Name, nullable: true).Description("The name of the character.");
            Field<AlignmentEnum>("alignment", "The alignment of the character", resolve: ctx => ctx.Source.Alignment);
            
            Field<ListGraphType<CharacterInterface>>("friends");
            Field<ListGraphType<EpisodeEnum>>("appearsIn", "Which movie they appear in.");
        }
    }
}

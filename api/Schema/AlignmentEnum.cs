using GraphQL.Types;
using StarWars.API.Models;

namespace StarWars.API.Schema;

public class AlignmentEnum : EnumerationGraphType<Alignment>
{
    public AlignmentEnum()
    {
        Name = "Alignment";
        Description = "Force spectrum alignment";
    }
}

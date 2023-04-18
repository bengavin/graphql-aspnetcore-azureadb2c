using System.ComponentModel;

namespace StarWars.API.Models;

public enum Alignment
{
    [Description("Unknown")]
    UNKNOWN = 0,
    
    [Description("Dark Side")]
    DARK = 1,

    [Description("Neutral")]
    NEUTRAL = 2,

    [Description("Light Side")]
    LIGHT = 3
}

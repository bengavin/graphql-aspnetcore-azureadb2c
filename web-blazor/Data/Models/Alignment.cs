using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StarWars.UI.Blazor.Data.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Alignment
    {
        [Description("")]
        UNKNOWN = 0,
        
        [Description("Dark Side")]
        DARK = 1,

        [Description("Neutral")]
        NEUTRAL = 2,

        [Description("Light Side")]
        LIGHT = 3
    }
}

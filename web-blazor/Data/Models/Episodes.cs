using System.ComponentModel;
using System.Text.Json.Serialization;

namespace StarWars.UI.Blazor.Data.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Episodes
    {
        [Description("The Phantom Menace")]
        PHANTOM = 1,
        [Description("Attack of the Clones")]
        ATTACKOFCLONES = 2,
        [Description("Revenge of the Sith")]
        REVENGEOFSITH = 3,
        [Description("A New Hope")]
        NEWHOPE = 4,
        [Description("The Empire Strikes Back")]
        EMPIRE = 5,
        [Description("Return of the Jedi")]
        RETURNOFJEDI = 6,
        [Description("The Force Awakens")]
        FORCEAWAKENS = 7,
        [Description("The Last Jedi")]
        LASTJEDI = 8,
        [Description("The Rise of Skywalker")]
        RISEOFSKYWALKER = 9,
    }
}

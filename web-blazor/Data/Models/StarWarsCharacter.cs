using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StarWars.UI.Blazor.Data.Models
{
    public abstract class StarWarsCharacter
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public Alignment Alignment { get; set; }

        public IEnumerable<StarWarsCharacter> Friends { get; set; }

        public IEnumerable<Episodes> AppearsIn { get; set; }
    }

    public class Human : StarWarsCharacter
    {
        [StringLength(100, ErrorMessage = "{0} must be no more than 100 characters")]
        public string HomePlanet { get; set; }
    }

    public class Droid : StarWarsCharacter
    {
        [StringLength(75, ErrorMessage = "{0} must be no more than 75 characters")]
        public string PrimaryFunction { get; set; }
    }
}

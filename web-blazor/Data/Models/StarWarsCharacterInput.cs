using System;
using System.Collections.Generic;

namespace StarWars.UI.Blazor.Data.Models
{
    public abstract class StarWarsCharacterInput
    {
        public string Name { get; set; }
        public Alignment Alignment { get; set; }
        public IEnumerable<string> Friends { get; set; }
        public IEnumerable<Episodes> AppearsIn { get; set; }
    }

    public class HumanInput : StarWarsCharacterInput
    {
        public string HomePlanet { get; set; }
    }

    public class DroidInput : StarWarsCharacterInput
    {
        public string PrimaryFunction { get; set; }
    }

}


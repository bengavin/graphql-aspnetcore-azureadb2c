using System;
namespace StarWars.UI.Blazor.Data.Models
{
    public class CharacterMutationResponse
    {
        public Human CreateHuman { get; set; }
        public Droid CreateDroid { get; set; }
        public Human UpdateHuman { get; set; }
        public Droid UpdateDroid { get; set; }
    }
}


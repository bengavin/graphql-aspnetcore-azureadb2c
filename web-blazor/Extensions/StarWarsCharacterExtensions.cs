using System;
using System.Linq;
using StarWars.UI.Blazor.Data.Models;

namespace StarWars.UI.Blazor.Extensions
{
    public static class StarWarsCharacterExtensions
    {
        public static HumanInput AsHumanInput(this StarWarsCharacter character)
        {
            return character is Human
                 ? new HumanInput
                 {
                     Alignment = character.Alignment,
                     AppearsIn = character.AppearsIn,
                     Friends = character.Friends?.Select(f => f.Id).ToList(),
                     HomePlanet = (character as Human).HomePlanet,
                     Name = character.Name
                 } : null;
        }

        public static DroidInput AsDroidInput(this StarWarsCharacter character)
        {
            return character is Droid
                 ? new DroidInput
                 {
                     Alignment = character.Alignment,
                     AppearsIn = character.AppearsIn,
                     Friends = character.Friends?.Select(f => f.Id).ToList(),
                     PrimaryFunction = (character as Droid).PrimaryFunction,
                     Name = character.Name
                 } : null;
        }
    }
}


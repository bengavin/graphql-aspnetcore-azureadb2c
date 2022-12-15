namespace StarWars.API.Models;

public abstract class StarWarsCharacter
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string[] Friends { get; set; } = new string[0];
    public int[] AppearsIn { get; set; } = new int[0];
}

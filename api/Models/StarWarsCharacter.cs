namespace StarWars.API.Models
{
    public abstract class StarWarsCharacter
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Alignment Alignment { get; set; }
        public string[] Friends { get; set; }
        public Episodes[] AppearsIn { get; set; }
    }

}

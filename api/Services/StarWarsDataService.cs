using System.Collections.Concurrent;
using Microsoft.Identity.Web;
using StarWars.API.Models;

namespace StarWars.API.Services;

public interface IStarWarsDataService
{
    Task<Human?> GetHumanByIdAsync(string id);
    Task<Droid?> GetDroidByIdAsync(string id);
    IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character);
    Human AddHuman(Human human);
}

public class StarWarsDataService : IStarWarsDataService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ConcurrentDictionary<string, UserStarWarsDataService> _userData = new ConcurrentDictionary<string, UserStarWarsDataService>();

    public StarWarsDataService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private UserStarWarsDataService GetUserDataService()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.GetNameIdentifierId();
        if (string.IsNullOrWhiteSpace(userId)) { throw new UnauthorizedAccessException("You must be authenticated to proceed"); }

        return _userData.GetOrAdd(userId, (id) => new UserStarWarsDataService());
    }

    public Human AddHuman(Human human)
    {
        return GetUserDataService().AddHuman(human);
    }

    public Task<Droid?> GetDroidByIdAsync(string id)
    {
        return GetUserDataService().GetDroidByIdAsync(id);
    }

    public IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character)
    {
        return GetUserDataService().GetFriends(character);
    }

    public Task<Human?> GetHumanByIdAsync(string id)
    {
        return GetUserDataService().GetHumanByIdAsync(id);
    }
}

public class UserStarWarsDataService : IStarWarsDataService
{
    private readonly List<Human> _humans = new List<Human>();
    private readonly List<Droid> _droids = new List<Droid>();

    public UserStarWarsDataService()
    {
        _humans.Add(new Human
        {
            Id = "1",
            Name = "Luke",
            Friends = new[] { "3", "4" },
            AppearsIn = new[] { 4, 5, 6 },
            HomePlanet = "Tatooine"
        });
        _humans.Add(new Human
        {
            Id = "2",
            Name = "Vader",
            AppearsIn = new[] { 4, 5, 6 },
            HomePlanet = "Tatooine"
        });

        _droids.Add(new Droid
        {
            Id = "3",
            Name = "R2-D2",
            Friends = new[] { "1", "4" },
            AppearsIn = new[] { 4, 5, 6 },
            PrimaryFunction = "Astromech"
        });
        _droids.Add(new Droid
        {
            Id = "4",
            Name = "C-3PO",
            AppearsIn = new[] { 4, 5, 6 },
            PrimaryFunction = "Protocol"
        });
    }

    public IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character)
    {
        if (character == null)
        {
            return Enumerable.Empty<StarWarsCharacter>();
        }

        var friends = new List<StarWarsCharacter>();
        var lookup = character.Friends;
        if (lookup != null)
        {
            friends.AddRange(_humans.Where(h => lookup.Contains(h.Id)));
            friends.AddRange(_droids.Where(d => lookup.Contains(d.Id)));
        }
        
        return friends;
    }

    public Task<Human?> GetHumanByIdAsync(string id)
    {
        return Task.FromResult(_humans.FirstOrDefault(h => h.Id == id));
    }

    public Task<Droid?> GetDroidByIdAsync(string id)
    {
        return Task.FromResult(_droids.FirstOrDefault(h => h.Id == id));
    }

    public Human AddHuman(Human human)
    {
        human.Id = Guid.NewGuid().ToString();
        _humans.Add(human);
        return human;
    }
}

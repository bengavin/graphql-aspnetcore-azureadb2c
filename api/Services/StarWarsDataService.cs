using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Web;
using StarWars.API.Models;

namespace StarWars.API.Services
{
    public interface IStarWarsDataService
    {
        Task<List<StarWarsCharacter>> GetCharactersAsync();
        Task<Human> GetHumanByIdAsync(string id);
        Task<Droid> GetDroidByIdAsync(string id);
        IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character);
        Human AddHuman(Human human);
        Human UpdateHuman(string id, Human human);
        Droid AddDroid(Droid droid);
        Droid UpdateDroid(string id, Droid droid);
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
            var userId = _httpContextAccessor.HttpContext.User?.GetNameIdentifierId();
            if (string.IsNullOrWhiteSpace(userId)) { throw new UnauthorizedAccessException("You must be authenticated to proceed"); }

            return _userData.GetOrAdd(userId, (id) => new UserStarWarsDataService());
        }

        public Human AddHuman(Human human)
        {
            return GetUserDataService().AddHuman(human);
        }

        public Task<Droid> GetDroidByIdAsync(string id)
        {
            return GetUserDataService().GetDroidByIdAsync(id);
        }

        public IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character)
        {
            return GetUserDataService().GetFriends(character);
        }

        public Task<Human> GetHumanByIdAsync(string id)
        {
            return GetUserDataService().GetHumanByIdAsync(id);
        }

        public Human UpdateHuman(string id, Human human)
        {
            return GetUserDataService().UpdateHuman(id, human);
        }

        public Droid AddDroid(Droid droid)
        {
            return GetUserDataService().AddDroid(droid);
        }

        public Droid UpdateDroid(string id, Droid droid)
        {
            return GetUserDataService().UpdateDroid(id, droid);
        }

        public Task<List<StarWarsCharacter>> GetCharactersAsync()
        {
            return GetUserDataService().GetCharactersAsync();
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
                Alignment = Alignment.LIGHT,
                Friends = new[] { "3", "4" },
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.RETURNOFJEDI },
                HomePlanet = "Tatooine"
            });
            _humans.Add(new Human
            {
                Id = "2",
                Name = "Vader",
                Alignment = Alignment.DARK,
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.RETURNOFJEDI },
                HomePlanet = "Tatooine"
            });

            _droids.Add(new Droid
            {
                Id = "3",
                Name = "R2-D2",
                Alignment = Alignment.LIGHT,
                Friends = new[] { "1", "4" },
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.RETURNOFJEDI },
                PrimaryFunction = "Astromech"
            });
            _droids.Add(new Droid
            {
                Id = "4",
                Name = "C-3PO",
                Alignment = Alignment.NEUTRAL,
                AppearsIn = new[] { Episodes.NEWHOPE, Episodes.EMPIRE, Episodes.RETURNOFJEDI },
                Friends = new[] { "1", "3" },
                PrimaryFunction = "Protocol"
            });
        }

        public IEnumerable<StarWarsCharacter> GetFriends(StarWarsCharacter character)
        {
            if (character == null)
            {
                return null;
            }

            return character.Friends
                            ?.Select(friendId => (StarWarsCharacter)_humans.FirstOrDefault(h => h.Id == friendId) 
                                             ?? _droids.FirstOrDefault(d => d.Id == friendId))
                            .OrderBy(f => f.Name)
                            .ToList() ?? new List<StarWarsCharacter>();
        }

        public Task<Human> GetHumanByIdAsync(string id)
        {
            return Task.FromResult(_humans.FirstOrDefault(h => h.Id == id));
        }

        public Task<Droid> GetDroidByIdAsync(string id)
        {
            return Task.FromResult(_droids.FirstOrDefault(h => h.Id == id));
        }

        public Human AddHuman(Human human)
        {
            human.Id = Guid.NewGuid().ToString();
            _humans.Add(human);

            if (human.Friends?.Any() ?? false)
            {
                // link up any friend references
                foreach (var friendId in human.Friends)
                {
                    var friendRef = (StarWarsCharacter)_humans.FirstOrDefault(h => h.Id == friendId)
                                  ?? _droids.FirstOrDefault(d => d.Id == friendId);
                    if (friendRef != null)
                    {
                        friendRef.Friends = (friendRef.Friends ?? new string[0]).Append(human.Id).ToArray();
                    }
                }
            }

            return human;
        }

        public Human UpdateHuman(string id, Human human)
        {
            // find the human
            var dataHuman = _humans.FirstOrDefault(h => string.Equals(id, h.Id, StringComparison.OrdinalIgnoreCase));
            if (dataHuman == null) { throw new ArgumentException($"Human with id {id} not found", "id"); }

            // Do the updates...
            var oldFriends = dataHuman.Friends;

            dataHuman.Alignment = human.Alignment;
            dataHuman.AppearsIn = human.AppearsIn;
            dataHuman.Friends = human.Friends;
            dataHuman.HomePlanet = human.HomePlanet;
            dataHuman.Name = human.Name;

            HarmonizeFriends(id, oldFriends, human.Friends);

            // return the updated item
            return dataHuman;
        }

        private void HarmonizeFriends(string id, string[] oldFriends, string[] newFriends)
        {
            // Find old friends that aren't in newFriends
            var friendsToRemove = oldFriends.Where(of => !newFriends.Any(nf => nf == of));
            foreach (var friendId in friendsToRemove)
            {
                var friendRef = (StarWarsCharacter)_humans.FirstOrDefault(h => h.Id == friendId)
                              ?? _droids.FirstOrDefault(h => h.Id == friendId);
                if (friendRef != null)
                {
                    friendRef.Friends = friendRef.Friends?.Except(new[] { id }).ToArray();
                }
            }

            // Add to new friends that aren't old friends
            var friendsToAdd = newFriends.Where(nf => !oldFriends.Any(of => of == nf));
            foreach (var friendId in friendsToAdd)
            {
                var friendRef = (StarWarsCharacter)_humans.FirstOrDefault(h => h.Id == friendId)
                              ?? _droids.FirstOrDefault(h => h.Id == friendId);
                if (friendRef != null)
                {
                    friendRef.Friends = (friendRef.Friends ?? new string[0]).Append(id).ToArray();
                }
            }
        }

        public Droid AddDroid(Droid droid)
        {
            droid.Id = Guid.NewGuid().ToString();
            _droids.Add(droid);

            if (droid.Friends?.Any() ?? false)
            {
                // link up any friend references
                foreach (var friendId in droid.Friends)
                {
                    var friendRef = (StarWarsCharacter)_humans.FirstOrDefault(h => h.Id == friendId)
                                  ?? _droids.FirstOrDefault(d => d.Id == friendId);
                    if (friendRef != null)
                    {
                        friendRef.Friends = (friendRef.Friends ?? new string[0]).Append(droid.Id).ToArray();
                    }
                }
            }

            return droid;
        }

        public Droid UpdateDroid(string id, Droid droid)
        {
            // find the Droid
            var dataDroid = _droids.FirstOrDefault(h => string.Equals(id, h.Id, StringComparison.OrdinalIgnoreCase));
            if (dataDroid == null) { throw new ArgumentException($"Droid with id {id} not found", "id"); }

            // Do the updates...
            var oldFriends = dataDroid.Friends;

            dataDroid.Alignment = droid.Alignment;
            dataDroid.AppearsIn = droid.AppearsIn;
            dataDroid.Friends = droid.Friends;
            dataDroid.PrimaryFunction = droid.PrimaryFunction;
            dataDroid.Name = droid.Name;

            HarmonizeFriends(id, oldFriends, droid.Friends);

            // return the updated item
            return dataDroid;
        }

        public Task<List<StarWarsCharacter>> GetCharactersAsync()
        {
            return Task.FromResult(
                _humans.Cast<StarWarsCharacter>()
                       .Concat(_droids)
                       .OrderBy(c => c.Name)
                       .ToList());
        }
    }
}

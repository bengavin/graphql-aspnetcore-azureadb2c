using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using StarWars.UI.Blazor.Data.Models;
using StarWars.UI.Blazor.Extensions;
using System;

namespace StarWars.UI.Blazor.Data
{
    public class StarWarsDataService
    {
        private class CharacterJsonConverter : GraphQLTypeJsonConverter<StarWarsCharacter>
        {
            public CharacterJsonConverter(JsonSerializerOptions options) : base(options)
            {
                
            }

            protected override (Type, JsonConverterDelegate) ResolveType(string graphQlTypeName)
            {
                return graphQlTypeName switch {
                    "Human" => (typeof(Human), DeserializeHuman),
                    "Droid" => (typeof(Droid), DeserializeDroid),
                    _ => (null, null)
                };
            }

            private StarWarsCharacter DeserializeCharacter(StarWarsCharacter character, Dictionary<string, JsonConverterDelegate> mappers, ref Utf8JsonReader reader)
            {
                while (reader.Read())
                {
                    // We're at the end of the object, kthxbai
                    if (reader.TokenType == JsonTokenType.EndObject) { return character; }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        continue; // we need to get to a property name
                    }

                    var propName = reader.GetString();
                    if (mappers.ContainsKey(propName))
                    {
                        reader.Read();
                        mappers[propName](character, ref reader);
                    }
                }

                return character;
            }

            private readonly JsonConverterDelegate IdSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => { instance.Id = reader.GetString(); return reader; };
            private readonly JsonConverterDelegate NameSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => { instance.Name = reader.GetString(); return reader; };
            private readonly JsonConverterDelegate HomePlanetSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => { ((Human)instance).HomePlanet = reader.GetString(); return reader; };
            private readonly JsonConverterDelegate PrimaryFunctionSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => { ((Droid)instance).PrimaryFunction = reader.GetString(); return reader; };
            private readonly JsonConverterDelegate AlignmentSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => 
            {
                instance.Alignment = JsonSerializer.Deserialize<Alignment>(ref reader);
                return reader;
            };

            private readonly JsonConverterDelegate EpisodesSetter = (StarWarsCharacter instance, ref Utf8JsonReader reader) => 
            {
                instance.AppearsIn = JsonSerializer.Deserialize<Episodes[]>(ref reader);

                return reader;
            };

            private Utf8JsonReader FriendsSetter(StarWarsCharacter instance, ref Utf8JsonReader reader)
            {
                if (reader.TokenType == JsonTokenType.StartArray && reader.Read())
                {
                    var friends = new List<StarWarsCharacter>();
                    
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        var friend = base.Read(ref reader, typeof(StarWarsCharacter), _options);
                        friends.Add(friend);
                        reader.Read(); // need to consume the 'interstitial' bits in the array
                    }

                    instance.Friends = friends.ToArray();
                }

                return reader;
            }

            private Dictionary<string, JsonConverterDelegate> CharacterFuncs => new Dictionary<string, JsonConverterDelegate>
            {
                {"id", IdSetter},
                {"name", NameSetter},
                {"alignment", AlignmentSetter},
                {"friends", FriendsSetter},
                {"appearsIn", EpisodesSetter }
            };
            
            private Utf8JsonReader DeserializeHuman(StarWarsCharacter character, ref Utf8JsonReader reader)
            {
                var funcs = CharacterFuncs;
                funcs.Add("homePlanet", HomePlanetSetter);

                DeserializeCharacter(character, funcs, ref reader);
                return reader;
            }

            private Utf8JsonReader DeserializeDroid(StarWarsCharacter character, ref Utf8JsonReader reader)
            {
                var funcs = CharacterFuncs;
                funcs.Add("primaryFunction", PrimaryFunctionSetter);

                DeserializeCharacter(character, funcs, ref reader);

                return reader;
            }
        }

        private readonly GraphQLHttpClient _graphqlClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IConfiguration _configuration; 

        public StarWarsDataService(IConfiguration configuration, ITokenAcquisition tokenAcquisition)
        {
            _configuration = configuration;
            var serializerOptions = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                IncludeFields = false,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            serializerOptions.Converters.Add(new CharacterJsonConverter(new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                IgnoreNullValues = true,
                IncludeFields = false,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            _graphqlClient = new GraphQLHttpClient(
                configuration["StarWarsDataService:BaseUri"], 
                new SystemTextJsonSerializer(serializerOptions));
            ((System.Net.Http.HttpClientHandler)_graphqlClient.Options.HttpMessageHandler).ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            _tokenAcquisition = tokenAcquisition;
        }

        private const string CHARACTERS_OPERATION = "characterList";
        private const string CHARACTER_CORE_FIELDS_FRAGMENT = @"
            fragment CoreFields on Character {
                id
                name
                alignment
                __typename
            }
            ";

        private static readonly string CHARACTER_LIST_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            query {CHARACTERS_OPERATION} {{
                characters {{
                    ... on Character {{
                        ...CoreFields
                    }}
                    ... on Human {{
                        homePlanet
                    }}
                    ... on Droid {{
                        primaryFunction
                    }}
                }}
            }}
            ";

        private static readonly string GET_HUMAN_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            query getHuman($id: ID!) {{
                human(id: $id) {{
                    ...CoreFields
                    appearsIn
                    friends {{
                        ...CoreFields
                    }}
                    homePlanet
                }}
            }}";

        private static readonly string GET_DROID_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            query getDroid($id: ID!) {{
                droid(id: $id) {{
                    ...CoreFields
                    appearsIn
                    friends {{
                        ...CoreFields
                    }}
                    primaryFunction
                }}
            }}";

        private static readonly string ADD_HUMAN_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            mutation addHuman($human: HumanInput!) {{
                createHuman(human: $human) {{
                    ...CoreFields
                }}
            }}";

        private static readonly string ADD_DROID_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            mutation addDroid($droid: DroidInput!) {{
                createDroid(droid: $droid) {{
                    ...CoreFields
                }}
            }}";

        private static readonly string UPDATE_HUMAN_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            mutation updateHuman($id: ID!, $human: HumanInput!) {{
                updateHuman(id: $id, human: $human) {{
                    ...CoreFields
                }}
            }}";

        private static readonly string UPDATE_DROID_QUERY = $@"{CHARACTER_CORE_FIELDS_FRAGMENT}
            mutation updateDroid($id: ID!, $droid: DroidInput!) {{
                updateDroid(id: $id, droid: $droid) {{
                    ...CoreFields
                }}
            }}";

        public async Task<List<StarWarsCharacter>> GetCharactersAsync()
        {
            await PrepareAuthenticatedClient();

            var request = new GraphQLRequest 
            {
                Query = CHARACTER_LIST_QUERY,
                OperationName = CHARACTERS_OPERATION
            };

            var response = await _graphqlClient.SendQueryAsync<CharacterListResponse>(request);
            return response.Data.Characters;
        }

        public async Task<StarWarsCharacter> GetCharacterAsync(string type, string id)
        {
            await PrepareAuthenticatedClient();

            if (string.Equals("new", id, StringComparison.OrdinalIgnoreCase))
            {
                return type switch
                {
                    "Human" => new Human { },
                    "Droid" => new Droid { },
                    _ => throw new ArgumentException("Unsupported character type")
                };
            }

            (string query, string operation, Func<CharacterResponse, StarWarsCharacter> extract) = type switch {
                "Human" => (GET_HUMAN_QUERY, "getHuman", (Func<CharacterResponse, StarWarsCharacter>)(r => r.Human)),
                "Droid" => (GET_DROID_QUERY, "getDroid", r => r.Droid),
                _ => throw new ArgumentException("Unsupported character type")
            };
            
            var request = new GraphQLRequest 
            {
                Query = query,
                OperationName = operation,
                Variables = new 
                {
                    id
                }
            };

            var response = await _graphqlClient.SendQueryAsync<CharacterResponse>(request);
            return extract(response.Data);
        }

        public async Task<StarWarsCharacter> AddCharacterAsync(StarWarsCharacter character)
        {
            await PrepareAuthenticatedClient();

            (string query, string operation, Func<CharacterMutationResponse, StarWarsCharacter> extract) = character switch
            {
                Human => (ADD_HUMAN_QUERY, "addHuman", (Func<CharacterMutationResponse, StarWarsCharacter>)(r => r.UpdateHuman)),
                Droid => (ADD_DROID_QUERY, "addDroid", r => r.UpdateDroid),
                _ => throw new ArgumentException("Unsupported character type")
            };

            var request = new GraphQLRequest
            {
                Query = query,
                OperationName = operation,
                Variables = new
                {
                    id = character.Id,
                    human = character.AsHumanInput(),
                    droid = character.AsDroidInput()
                }
            };

            var response = await _graphqlClient.SendMutationAsync<CharacterMutationResponse>(request);
            return extract(response.Data);
        }

        public async Task<StarWarsCharacter> UpdateCharacterAsync(StarWarsCharacter character)
        {
            await PrepareAuthenticatedClient();

            (string query, string operation, Func<CharacterMutationResponse, StarWarsCharacter> extract) = character switch
            {
                Human => (UPDATE_HUMAN_QUERY, "updateHuman", (Func<CharacterMutationResponse, StarWarsCharacter>)(r => r.UpdateHuman)),
                Droid => (UPDATE_DROID_QUERY, "updateDroid", r => r.UpdateDroid),
                _ => throw new ArgumentException("Unsupported character type")
            };

            var request = new GraphQLRequest
            {
                Query = query,
                OperationName = operation,
                Variables = new
                {
                    id = character.Id,
                    human = character.AsHumanInput(),
                    droid = character.AsDroidInput()
                }
            };

            var response = await _graphqlClient.SendMutationAsync<CharacterMutationResponse>(request);
            return extract(response.Data);
        }

        private async Task PrepareAuthenticatedClient()
        {
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(_configuration["StarWarsDataService:Scopes"].Split(new [] { ' ' }));

            Debug.WriteLine($"access token-{accessToken}");
            _graphqlClient.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _graphqlClient.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}
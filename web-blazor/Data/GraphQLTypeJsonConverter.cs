using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StarWars.UI.Blazor.Data
{
    public abstract class GraphQLTypeJsonConverter<T> : JsonConverter<T>
    {
        protected readonly JsonSerializerOptions _options;
        private static readonly string DiscriminatorPropertyName = "__typename";

        protected GraphQLTypeJsonConverter(JsonSerializerOptions options)
        {
            _options = options;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeof(T).IsAssignableFrom(typeToConvert);

        public override T Read(
            ref Utf8JsonReader reader, 
            Type typeToConvert, 
            JsonSerializerOptions options)
        {
            Utf8JsonReader propertyReader = reader;

            if (propertyReader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Converter must be invoked for JSON object.  Current Token Type - {propertyReader.TokenType}.");
            }

            while (propertyReader.Read())
            {
                // Is this a property?
                if (propertyReader.TokenType != JsonTokenType.PropertyName) { continue; }

                // Is this the type discriminator property?
                var propertyName = propertyReader.GetString();
                if (!propertyName.Equals(DiscriminatorPropertyName, StringComparison.OrdinalIgnoreCase)) { continue; }

                // Is the discriminator property value something we can understand?
                propertyReader.Read();
                if (propertyReader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"{DiscriminatorPropertyName} value is invalid, expected String, received {propertyReader.TokenType}");
                }

                // What type should we be deserializing?
                var typeName = propertyReader.GetString();
                var (objectType, deserializer) = ResolveType(typeName);
                if (objectType == null)
                {
                    throw new JsonException($"Unsupported object type ({typeName}) - Ensure type mapping is created in JsonConverter");
                }

                // Is that a type that can be safely returned from here?
                if (!typeof(T).IsAssignableFrom(objectType))
                {
                    throw new JsonException($"Unsupported object type ({typeName}) - Type not derived from {typeof(T)}");
                }

                var returnValue = (T)Activator.CreateInstance(objectType);

                reader = deserializer(returnValue, ref reader);

                return returnValue;
            }

            throw new JsonException($"{DiscriminatorPropertyName} not present, cannot deserialize resulting JSON.  Please explicitly include {DiscriminatorPropertyName} in the GraphQL query.");
        }

        protected delegate Utf8JsonReader JsonConverterDelegate(T instance, ref Utf8JsonReader reader);

        public override void Write(
            Utf8JsonWriter writer, 
            T input, 
            JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, input, _options);
        }

        protected abstract (Type, JsonConverterDelegate) ResolveType(string graphQlTypeName);

    }
}
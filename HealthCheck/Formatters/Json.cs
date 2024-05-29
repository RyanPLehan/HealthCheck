using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthCheck.Formatters
{
    public static class Json
    {
        private static JsonSerializerOptions JsonSerializerObjectConverterOptions = null;


        public const string JSON_CONTENT_TYPE = @"application/json";

        public static JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
        };


        public static JsonSerializerOptions CreateOptionsWithObjectConverter()
        {
            if (JsonSerializerObjectConverterOptions == null)
            {
                JsonSerializerObjectConverterOptions = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                };

                JsonSerializerObjectConverterOptions.Converters.Add(new JsonObjectConverter());
            }

            return JsonSerializerObjectConverterOptions;
        }

        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, null);
        }

        public static T Deserialize<T>(string json, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions;
            return String.IsNullOrWhiteSpace(json) ? default(T) : JsonSerializer.Deserialize<T>(json, jsonOptions);
        }

        public static IEnumerable<T> Deserialize<T>(IEnumerable<string> json)
        {
            return Deserialize<T>(json, null);
        }

        public static IEnumerable<T> Deserialize<T>(IEnumerable<string> json, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions;
            return json == null ? Enumerable.Empty<T>() : json.Where(x => !String.IsNullOrWhiteSpace(x))
                                                              .Select(x => JsonSerializer.Deserialize<T>(x, jsonOptions))
                                                              .ToArray();
        }

        public static string Serialize(object value)
        {
            return Serialize(value, null);
        }

        public static string Serialize(object value, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonOptions = jsonSerializerOptions ?? DefaultJsonSerializerOptions;
            return value == null ? null : JsonSerializer.Serialize(value, jsonOptions);
        }
    }
}

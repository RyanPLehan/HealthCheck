using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HealthCheck.Formatters
{

    /// <summary>
    /// The MS System.Text.Json JsonConverter does not handle objects like the NewtonSoft Json does
    /// This will attempt to convert the value as best as possible
    /// </summary>
    /// <remarks>
    /// Idea came from the following sources:
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/converters-how-to?pivots=dotnet-7-0#deserialize-inferred-types-to-object-properties
    /// https://github.com/dotnet/runtime/issues/29960
    /// </remarks>
    internal class JsonObjectConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            object? ret;

            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                case JsonTokenType.None:
                    ret = null;
                    break;

                case JsonTokenType.True:
                case JsonTokenType.False:
                    ret = reader.GetBoolean();
                    break;

                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int i))
                        ret = i;
                    else if (reader.TryGetInt64(out long lng))
                        ret = lng;
                    else if (reader.TryGetDecimal(out decimal dml))
                        ret = dml;
                    else
                        ret = reader.GetDouble();
                    break;

                case JsonTokenType.String:
                    if (reader.TryGetDateTime(out DateTime dt))
                        ret = dt;
                    else
                        ret = reader.GetString();
                    break;

                default:
                    ret = JsonDocument.ParseValue(ref reader).RootElement.Clone();
                    break;
            }

            return ret;
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}

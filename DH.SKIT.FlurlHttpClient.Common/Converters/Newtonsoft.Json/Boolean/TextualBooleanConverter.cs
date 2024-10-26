using System;
using System.Text.RegularExpressions;

namespace Newtonsoft.Json.Converters.Common
{
    /// <summary>
    /// 一个 JSON 转换器，可针对指定适配类型做如下形式的对象转换。
    /// <code>
    ///   .NET → bool Foo { get; } = true;
    ///   JSON → { "Foo": "true" }
    /// </code>
    /// 
    /// 适配类型：
    /// <code>  <see cref="bool"/> <see cref="bool"/>?</code>
    /// </summary>
    public sealed partial class TextualBooleanConverter : JsonConverter
    {
        private static readonly JsonConverter<bool?> _converter = new InternalTextualBooleanConverter();

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(bool) == objectType ||
                   typeof(bool?) == objectType;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            bool? result = _converter.ReadJson(reader, objectType, (bool?)existingValue, (bool?)existingValue is not null, serializer);
            if (objectType == typeof(bool))
                return result.GetValueOrDefault();
            return result;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            _converter.WriteJson(writer, (bool?)value, serializer);
        }
    }

    partial class TextualBooleanConverter
    {
        private sealed class InternalTextualBooleanConverter : JsonConverter<bool?>
        {
            private const string TRUE_VALUE = "true";
            private const string FALSE_VALUE = "false";

            private static readonly Regex TRUE_REGEX = new Regex("^([T|t]rue|TRUE)$", RegexOptions.Compiled);
            private static readonly Regex FALSE_REGEX = new Regex("^([F|f]alse|FALSE)$", RegexOptions.Compiled);

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return true; }
            }

            public override bool? ReadJson(JsonReader reader, Type objectType, bool? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return existingValue;
                }
                else if (reader.TokenType == JsonToken.Boolean)
                {
                    return serializer.Deserialize<bool?>(reader);
                }
                else if (reader.TokenType == JsonToken.String)
                {
                    string? value = serializer.Deserialize<string>(reader);
                    if (string.IsNullOrEmpty(value))
                        return existingValue;

                    if (TRUE_REGEX.IsMatch(value))
                        return true;
                    else if (FALSE_REGEX.IsMatch(value))
                        return false;

                    throw new JsonSerializationException($"Could not parse String '{value}' to Boolean.");
                }

                throw new JsonSerializationException($"Unexpected token type '{reader.TokenType}' when deserializing. Path '{reader.Path}'.");
            }

            public override void WriteJson(JsonWriter writer, bool? value, JsonSerializer serializer)
            {
                if (value.HasValue)
                    writer.WriteValue(value.Value ? TRUE_VALUE : FALSE_VALUE);
                else
                    writer.WriteNull();
            }
        }
    }
}

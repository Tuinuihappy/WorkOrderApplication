using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkOrderApplication.API.Helpers;

public class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    private static readonly string[] SupportedFormats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ"
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (string.IsNullOrWhiteSpace(str))
            return default;

        if (DateTime.TryParseExact(str, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            return parsed.ToUniversalTime();

        if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsed))
            return parsed.ToUniversalTime();

        throw new JsonException($"❌ Invalid DateTime format: {str}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
}

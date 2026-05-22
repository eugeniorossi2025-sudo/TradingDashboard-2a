using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gamebot.Helpers
{

    public class Double2DecimalConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetDouble();
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteRawValue(value.ToString("0.00", CultureInfo.InvariantCulture), false);
        }
    }
}

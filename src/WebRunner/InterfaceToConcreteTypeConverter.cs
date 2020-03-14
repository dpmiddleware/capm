using System;
using System.Text.Json;

namespace WebRunner
{
    // This class is needed since the default behaviour changed in aspnet core due to
    // system.text.json only serializing the interface members of interfaces, instead
    // of all properties on the concrete types, to avoid accidentally exposing data.
    // However, in this sample we relied on the previous behaviour, so we need to add
    // special converters for the types we serialize using interfaces.
    internal class InterfaceToConcreteTypeConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Net.Leksi.Dto
{
    internal class SupportedTypeConverter<T> : JsonConverter<T>
    {
        private readonly DtoJsonConverterFactory _factory;

        public SupportedTypeConverter(DtoJsonConverterFactory factory)
        {
            _factory = factory;
        }

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (typeof(T).IsEnum)
            {
                writer.WriteStringValue(value!.ToString());
            }
        }
    }
}
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CoreApi.Configuracion;

// Lee y escribe los enums por su nombre ("Pami"), sin distinguir mayúsculas.
// A diferencia de JsonStringEnumConverter, rechaza números (99) y combinaciones ("Pami, Osde" sería 1 | 2 = SwissMedical).
public class EnumComoTexto : JsonConverterFactory
{
    public override bool CanConvert(Type tipo) => tipo.IsEnum;

    public override JsonConverter CreateConverter(Type tipo, JsonSerializerOptions opciones) =>
        (JsonConverter)Activator.CreateInstance(typeof(Conversor<>).MakeGenericType(tipo))!;

    class Conversor<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader lector, Type tipo, JsonSerializerOptions opciones)
        {
            var texto = lector.TokenType == JsonTokenType.String ? lector.GetString() : null;
            var nombre = Enum.GetNames<T>().FirstOrDefault(n => string.Equals(n, texto, StringComparison.OrdinalIgnoreCase));
            if (nombre is null) throw new JsonException();   // ErroresDeEntrada lo convierte en un 400 con los valores válidos
            return Enum.Parse<T>(nombre);
        }

        public override void Write(Utf8JsonWriter escritor, T valor, JsonSerializerOptions opciones) =>
            escritor.WriteStringValue(valor.ToString());
    }

    // Con un conversor propio, OpenAPI ya no sabe la forma del enum: se la indicamos (texto con los nombres válidos)
    public static Task DescribirEnOpenApi(OpenApiSchema esquema, OpenApiSchemaTransformerContext contexto, CancellationToken cancelacion)
    {
        var tipo = Nullable.GetUnderlyingType(contexto.JsonTypeInfo.Type) ?? contexto.JsonTypeInfo.Type;
        if (tipo.IsEnum)
        {
            esquema.Type = JsonSchemaType.String;
            esquema.Enum = Enum.GetNames(tipo).Select(n => (JsonNode)JsonValue.Create(n)).ToList();
        }
        return Task.CompletedTask;
    }
}

using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClassicApi
{
    // Quita los espacios de los extremos de todos los textos al leer el JSON ("  Ana  " pasa a ser "Ana"),
    // así las validaciones de Paciente ya ven el valor limpio. Rechaza números u objetos donde se espera texto.
    public class RecortarTexto : JsonConverter
    {
        public override bool CanConvert(Type tipo) => tipo == typeof(string);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader lector, Type tipo, object existente, JsonSerializer serializador)
        {
            if (lector.TokenType == JsonToken.Null) return null;
            if (lector.TokenType != JsonToken.String) throw new JsonSerializationException("Se esperaba un texto.");
            return ((string)lector.Value).Trim();
        }

        public override void WriteJson(JsonWriter escritor, object valor, JsonSerializer serializador) =>
            throw new NotSupportedException();
    }

    // Acepta fechas solo con formato aaaa-mm-dd. Sin esto Newtonsoft acepta "01/05/1990" como 5 de enero
    // y fechas con hora. Requiere DateParseHandling.None para recibir el texto original.
    public class FechaSinHora : JsonConverter
    {
        public override bool CanConvert(Type tipo) => tipo == typeof(DateTime) || tipo == typeof(DateTime?);

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader lector, Type tipo, object existente, JsonSerializer serializador)
        {
            if (lector.TokenType == JsonToken.Null && tipo == typeof(DateTime?)) return null;
            DateTime fecha;
            if (lector.TokenType == JsonToken.String &&
                DateTime.TryParseExact((string)lector.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
                return fecha;
            throw new JsonSerializationException("Fecha no válida.");   // el controlador lo traduce a un mensaje claro
        }

        public override void WriteJson(JsonWriter escritor, object valor, JsonSerializer serializador) =>
            throw new NotSupportedException();
    }

    // Lee y escribe los enums por su nombre ("Pami"), sin distinguir mayúsculas.
    // A diferencia de StringEnumConverter, rechaza números (99) y combinaciones ("Pami, Osde" sería 1 | 2 = SwissMedical).
    public class EnumComoTexto : StringEnumConverter
    {
        public override object ReadJson(JsonReader lector, Type tipo, object existente, JsonSerializer serializador)
        {
            var tipoEnum = Nullable.GetUnderlyingType(tipo) ?? tipo;
            if (lector.TokenType == JsonToken.Null && tipoEnum != tipo) return null;

            var texto = lector.TokenType == JsonToken.String ? (string)lector.Value : null;
            var nombre = Enum.GetNames(tipoEnum).FirstOrDefault(n => string.Equals(n, texto, StringComparison.OrdinalIgnoreCase));
            if (nombre == null) throw new JsonSerializationException("Valor de enum no válido.");   // el controlador lo traduce a un mensaje claro
            return Enum.Parse(tipoEnum, nombre);
        }
    }
}

using System.Text.Json.Serialization;

namespace CoreApi.Models;

// Opciones válidas de obra social (se muestran como texto en el JSON y en Swagger)
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObraSocial { NoTiene, Pami, Osde, SwissMedical, Osecac }

// Paciente de la clínica (el DNI es el identificador y no puede repetirse)
public record Paciente(
    string Nombre,
    string Apellido,
    string Dni,
    DateOnly FechaNacimiento,
    string Telefono,
    string Correo,
    ObraSocial ObraSocial);

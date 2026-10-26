using System.ComponentModel.DataAnnotations;

namespace CoreApi.Models;

// Opciones válidas de obra social (se reciben y devuelven como texto: ver ConfigureHttpJsonOptions en Program.cs)
public enum ObraSocial { NoTiene, Pami, Osde, SwissMedical, Osecac }

// Paciente de la clínica (el DNI es el identificador y no puede repetirse).
// Los atributos se validan en PacientesEndpoints.Validar y además se publican en el contrato OpenAPI.
public record Paciente(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    [property: RegularExpression(ReglasPaciente.PatronNombre, ErrorMessage = "El nombre solo puede tener letras, espacios, apóstrofes y guiones.")]
    string Nombre,

    [property: Required(ErrorMessage = "El apellido es obligatorio.")]
    [property: StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
    [property: RegularExpression(ReglasPaciente.PatronNombre, ErrorMessage = "El apellido solo puede tener letras, espacios, apóstrofes y guiones.")]
    string Apellido,

    [property: Required(ErrorMessage = "El DNI es obligatorio.")]
    [property: RegularExpression(ReglasPaciente.PatronDni, ErrorMessage = "El DNI debe tener 7 u 8 dígitos, sin puntos ni letras.")]
    string Dni,

    // Nullable para distinguir "no se envió" de una fecha real
    [property: Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [property: FechaDeNacimiento]
    DateOnly? FechaNacimiento,

    [property: Required(ErrorMessage = "El teléfono es obligatorio.")]
    [property: RegularExpression(ReglasPaciente.PatronTelefono, ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos (se permiten +, espacios y guiones).")]
    string Telefono,

    [property: Required(ErrorMessage = "El correo es obligatorio.")]
    [property: StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
    [property: RegularExpression(ReglasPaciente.PatronCorreo, ErrorMessage = "El correo no tiene un formato válido.")]
    string Correo,

    // Opcional: si no se envía queda NoTiene
    [property: EnumDataType(typeof(ObraSocial), ErrorMessage = "La obra social no es válida.")]
    ObraSocial ObraSocial = ObraSocial.NoTiene);

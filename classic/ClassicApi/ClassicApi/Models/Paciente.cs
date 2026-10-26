using System;
using System.ComponentModel.DataAnnotations;

namespace ClassicApi.Models
{
    // Opciones válidas de obra social (se reciben y devuelven como texto: ver App_Start/ConversoresJson.cs)
    public enum ObraSocial { NoTiene, Pami, Osde, SwissMedical, Osecac }

    /// <summary>Paciente de la clínica (el DNI es el identificador y no puede repetirse).</summary>
    public class Paciente
    {
        /// <summary>Nombre: letras, espacios, apóstrofes y guiones (obligatorio, hasta 50).</summary>
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        [RegularExpression(ReglasPaciente.PatronNombre, ErrorMessage = "El nombre solo puede tener letras, espacios, apóstrofes y guiones.")]
        public string Nombre { get; set; }

        /// <summary>Apellido: letras, espacios, apóstrofes y guiones (obligatorio, hasta 50).</summary>
        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        [RegularExpression(ReglasPaciente.PatronNombre, ErrorMessage = "El apellido solo puede tener letras, espacios, apóstrofes y guiones.")]
        public string Apellido { get; set; }

        /// <summary>DNI: 7 u 8 dígitos sin puntos (obligatorio, único).</summary>
        [Required(ErrorMessage = "El DNI es obligatorio.")]
        [RegularExpression(ReglasPaciente.PatronDni, ErrorMessage = ReglasPaciente.MensajeDni)]
        public string Dni { get; set; }

        /// <summary>Fecha de nacimiento, formato aaaa-mm-dd: no futura ni de hace más de 120 años (obligatoria).</summary>
        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [FechaDeNacimiento]
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Teléfono: 8 a 15 dígitos, se permiten +, espacios y guiones (obligatorio).</summary>
        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(ReglasPaciente.PatronTelefono, ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos (se permiten +, espacios y guiones).")]
        public string Telefono { get; set; }

        /// <summary>Correo electrónico (obligatorio, hasta 100).</summary>
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        [RegularExpression(ReglasPaciente.PatronCorreo, ErrorMessage = "El correo no tiene un formato válido.")]
        public string Correo { get; set; }

        /// <summary>Obra social: NoTiene, Pami, Osde, SwissMedical u Osecac (opcional, por defecto NoTiene).</summary>
        [EnumDataType(typeof(ObraSocial), ErrorMessage = "La obra social no es válida.")]
        public ObraSocial ObraSocial { get; set; }
    }
}

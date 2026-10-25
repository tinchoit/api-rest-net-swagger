using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ClassicApi.Models
{
    // Opciones válidas de obra social (se muestran como texto en el JSON y en Swagger)
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObraSocial { NoTiene, Pami, Osde, SwissMedical, Osecac }

    /// <summary>Paciente de la clínica (el DNI es el identificador y no puede repetirse).</summary>
    public class Paciente
    {
        /// <summary>Nombre (obligatorio).</summary>
        [Required]
        public string Nombre { get; set; }

        /// <summary>Apellido (obligatorio).</summary>
        [Required]
        public string Apellido { get; set; }

        /// <summary>DNI sin puntos (obligatorio, único).</summary>
        [Required]
        public string Dni { get; set; }

        /// <summary>Fecha de nacimiento, formato aaaa-mm-dd (obligatoria).</summary>
        [Required]
        public DateTime? FechaNacimiento { get; set; }

        /// <summary>Teléfono de contacto (obligatorio).</summary>
        [Required]
        public string Telefono { get; set; }

        /// <summary>Correo electrónico (obligatorio).</summary>
        [Required]
        public string Correo { get; set; }

        /// <summary>Obra social: NoTiene, Pami, Osde, SwissMedical u Osecac.</summary>
        public ObraSocial ObraSocial { get; set; }
    }
}

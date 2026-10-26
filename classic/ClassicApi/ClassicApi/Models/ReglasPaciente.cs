using System;
using System.ComponentModel.DataAnnotations;

namespace ClassicApi.Models
{
    // Formatos válidos de los datos de un paciente (se usan en los atributos de Paciente y en la ruta {dni})
    public static class ReglasPaciente
    {
        public const string PatronDni = @"^[0-9]{7,8}$";                       // 7 u 8 dígitos, sin puntos
        public const string PatronNombre = @"^\p{L}+([ '\-]\p{L}+)*$";          // letras, con espacios, apóstrofes o guiones entre palabras
        public const string PatronTelefono = @"^\+?[0-9]([ \-]?[0-9]){7,14}$";  // 8 a 15 dígitos, con + inicial, espacios o guiones opcionales
        public const string PatronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        public const int EdadMaxima = 120;

        public const string MensajeDni = "El DNI debe tener 7 u 8 dígitos, sin puntos ni letras.";
    }

    // La fecha de nacimiento no puede ser futura ni de hace más de 120 años (el formato lo controla FechaSinHora)
    public class FechaDeNacimientoAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext contexto)
        {
            if (!(value is DateTime)) return ValidationResult.Success;   // la ausencia la controla [Required]

            var fecha = (DateTime)value;
            var campo = contexto.MemberName != null ? new[] { contexto.MemberName } : null;
            if (fecha > DateTime.Today)
                return new ValidationResult("La fecha de nacimiento no puede ser futura.", campo);
            if (fecha < DateTime.Today.AddYears(-ReglasPaciente.EdadMaxima))
                return new ValidationResult($"La fecha de nacimiento no puede ser de hace más de {ReglasPaciente.EdadMaxima} años.", campo);
            return ValidationResult.Success;
        }
    }
}

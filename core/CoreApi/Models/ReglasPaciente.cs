using System.ComponentModel.DataAnnotations;

namespace CoreApi.Models;

// Formatos válidos de los datos de un paciente (se usan en los atributos de Paciente y en la ruta /{dni})
public static class ReglasPaciente
{
    public const string PatronDni = @"^[0-9]{7,8}$";                       // 7 u 8 dígitos, sin puntos
    public const string PatronNombre = @"^\p{L}+([ '\-]\p{L}+)*$";          // letras, con espacios, apóstrofes o guiones entre palabras
    public const string PatronTelefono = @"^\+?[0-9]([ \-]?[0-9]){7,14}$";  // 8 a 15 dígitos, con + inicial, espacios o guiones opcionales
    public const string PatronCorreo = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const int EdadMaxima = 120;
}

// La fecha de nacimiento no puede ser futura ni de hace más de 120 años
public class FechaDeNacimientoAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext contexto)
    {
        if (value is not DateOnly fecha) return ValidationResult.Success;   // la ausencia la controla [Required]

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        string[]? campo = contexto.MemberName is { } nombre ? [nombre] : null;
        if (fecha > hoy) return new ValidationResult("La fecha de nacimiento no puede ser futura.", campo);
        if (fecha < hoy.AddYears(-ReglasPaciente.EdadMaxima))
            return new ValidationResult($"La fecha de nacimiento no puede ser de hace más de {ReglasPaciente.EdadMaxima} años.", campo);
        return ValidationResult.Success;
    }
}

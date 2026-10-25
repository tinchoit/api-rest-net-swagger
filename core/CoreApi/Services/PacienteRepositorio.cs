using System.Globalization;
using System.Text;
using CoreApi.Models;

namespace CoreApi.Services;

public interface IPacienteRepositorio
{
    IReadOnlyList<Paciente> ObtenerTodos();
    Paciente? ObtenerPorDni(string dni);
    IReadOnlyList<Paciente> Buscar(string? nombre, string? apellido);
    bool Agregar(Paciente paciente);                  // false si el DNI ya existe
    bool Actualizar(string dni, Paciente paciente);
    bool Eliminar(string dni);
}

// Los datos viven en una lista en memoria: se pierden al cerrar la aplicación
public class PacienteRepositorio : IPacienteRepositorio
{
    private readonly object _bloqueo = new();
    private readonly List<Paciente> _pacientes = new()
    {
        new("Juan", "Pérez", "30111222", new DateOnly(1985, 3, 12), "1155550001", "juan.perez@mail.com", ObraSocial.Osde),
        new("María", "Gómez", "28555666", new DateOnly(1979, 11, 4), "1155550002", "maria.gomez@mail.com", ObraSocial.SwissMedical),
        new("Carlos", "Ruiz", "20333444", new DateOnly(1950, 6, 25), "1155550003", "carlos.ruiz@mail.com", ObraSocial.Pami),
        new("Lucía", "Fernández", "40222111", new DateOnly(2000, 1, 30), "1155550004", "lucia.fernandez@mail.com", ObraSocial.NoTiene)
    };

    public IReadOnlyList<Paciente> ObtenerTodos() { lock (_bloqueo) return _pacientes.ToList(); }

    public Paciente? ObtenerPorDni(string dni) { lock (_bloqueo) return _pacientes.FirstOrDefault(p => p.Dni == dni); }

    public IReadOnlyList<Paciente> Buscar(string? nombre, string? apellido)
    {
        // Se comparan los textos sin acentos y sin importar mayúsculas o minúsculas
        var nombreBuscado = SinAcentos((nombre ?? "").Trim());
        var apellidoBuscado = SinAcentos((apellido ?? "").Trim());

        lock (_bloqueo)
        {
            return _pacientes.Where(p =>
                SinAcentos(p.Nombre).Contains(nombreBuscado, StringComparison.OrdinalIgnoreCase) &&
                SinAcentos(p.Apellido).Contains(apellidoBuscado, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    // Quita los acentos de un texto: "Gómez" pasa a ser "Gomez"
    private static string SinAcentos(string texto)
    {
        var descompuesto = texto.Normalize(NormalizationForm.FormD);
        var resultado = new StringBuilder();
        foreach (var letra in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(letra) != UnicodeCategory.NonSpacingMark)
                resultado.Append(letra);
        }
        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }

    public bool Agregar(Paciente paciente)
    {
        lock (_bloqueo)
        {
            if (_pacientes.Any(p => p.Dni == paciente.Dni)) return false;
            _pacientes.Add(paciente);
            return true;
        }
    }

    public bool Actualizar(string dni, Paciente paciente)
    {
        lock (_bloqueo)
        {
            var i = _pacientes.FindIndex(p => p.Dni == dni);
            if (i < 0) return false;
            _pacientes[i] = paciente;
            return true;
        }
    }

    public bool Eliminar(string dni) { lock (_bloqueo) return _pacientes.RemoveAll(p => p.Dni == dni) > 0; }
}

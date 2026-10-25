using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ClassicApi.Models
{
    // Repositorio en memoria con datos ficticios: se pierden al cerrar la aplicación.
    // Es estático porque Web API 2 no trae inyección de dependencias incorporada.
    public static class PacientesRepositorio
    {
        private static readonly object _bloqueo = new object();
        private static readonly List<Paciente> _pacientes = new List<Paciente>
        {
            new Paciente { Nombre = "Juan", Apellido = "Pérez", Dni = "30111222", FechaNacimiento = new DateTime(1985, 3, 12),
                Telefono = "1155550001", Correo = "juan.perez@mail.com", ObraSocial = ObraSocial.Osde },
            new Paciente { Nombre = "María", Apellido = "Gómez", Dni = "28555666", FechaNacimiento = new DateTime(1979, 11, 4),
                Telefono = "1155550002", Correo = "maria.gomez@mail.com", ObraSocial = ObraSocial.SwissMedical },
            new Paciente { Nombre = "Carlos", Apellido = "Ruiz", Dni = "20333444", FechaNacimiento = new DateTime(1950, 6, 25),
                Telefono = "1155550003", Correo = "carlos.ruiz@mail.com", ObraSocial = ObraSocial.Pami },
            new Paciente { Nombre = "Lucía", Apellido = "Fernández", Dni = "40222111", FechaNacimiento = new DateTime(2000, 1, 30),
                Telefono = "1155550004", Correo = "lucia.fernandez@mail.com", ObraSocial = ObraSocial.NoTiene }
        };

        public static IList<Paciente> ObtenerTodos() { lock (_bloqueo) return _pacientes.ToList(); }

        public static Paciente ObtenerPorDni(string dni) { lock (_bloqueo) return _pacientes.FirstOrDefault(p => p.Dni == dni); }

        public static IList<Paciente> Buscar(string nombre, string apellido)
        {
            // Se comparan los textos sin acentos y sin importar mayúsculas o minúsculas
            var nombreBuscado = SinAcentos((nombre ?? "").Trim());
            var apellidoBuscado = SinAcentos((apellido ?? "").Trim());

            lock (_bloqueo)
            {
                return _pacientes.Where(p =>
                    SinAcentos(p.Nombre).IndexOf(nombreBuscado, StringComparison.OrdinalIgnoreCase) >= 0 &&
                    SinAcentos(p.Apellido).IndexOf(apellidoBuscado, StringComparison.OrdinalIgnoreCase) >= 0)
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

        // Devuelve false si ya existe un paciente con ese DNI
        public static bool Agregar(Paciente paciente)
        {
            lock (_bloqueo)
            {
                if (_pacientes.Any(p => p.Dni == paciente.Dni)) return false;
                _pacientes.Add(paciente);
                return true;
            }
        }

        public static bool Actualizar(string dni, Paciente paciente)
        {
            lock (_bloqueo)
            {
                var i = _pacientes.FindIndex(p => p.Dni == dni);
                if (i < 0) return false;
                _pacientes[i] = paciente;
                return true;
            }
        }

        public static bool Eliminar(string dni) { lock (_bloqueo) return _pacientes.RemoveAll(p => p.Dni == dni) > 0; }
    }
}

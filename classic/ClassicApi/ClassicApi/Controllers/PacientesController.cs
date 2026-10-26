using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using ClassicApi.Models;

namespace ClassicApi.Controllers
{
    /// <summary>Gestión de pacientes de la clínica (ASP.NET Web API 2 - .NET Framework 4.8).</summary>
    [RoutePrefix("api/pacientes")]
    public class PacientesController : ApiController
    {
        /// <summary>Lista todos los pacientes.</summary>
        [HttpGet, Route("")]
        [ResponseType(typeof(IEnumerable<Paciente>))]
        public IHttpActionResult Get()
        {
            return Ok(PacientesRepositorio.ObtenerTodos());
        }

        /// <summary>Busca pacientes por nombre y/o apellido.</summary>
        /// <param name="nombre">Parte del nombre (opcional).</param>
        /// <param name="apellido">Parte del apellido (opcional).</param>
        [HttpGet, Route("buscar")]
        [ResponseType(typeof(IEnumerable<Paciente>))]
        public IHttpActionResult Buscar(string nombre = null, string apellido = null)
        {
            return Ok(PacientesRepositorio.Buscar(nombre, apellido));
        }

        /// <summary>Obtiene un paciente por DNI (400 si el DNI no tiene 7 u 8 dígitos).</summary>
        /// <param name="dni">DNI del paciente.</param>
        [HttpGet, Route("{dni}", Name = "ObtenerPacientePorDni")]
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult Get(string dni)
        {
            if (!DniValido(dni)) return DniInvalido();
            var paciente = PacientesRepositorio.ObtenerPorDni(dni);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        /// <summary>Crea un paciente (400 si algún dato no es válido, 409 si el DNI ya existe).</summary>
        [HttpPost, Route("")]
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult Post([FromBody] Paciente paciente)
        {
            // En Web API 2 hay que revisar el ModelState a mano (no existe [ApiController]).
            if (!ModelState.IsValid) return DatosInvalidos();
            if (paciente == null) return CuerpoFaltante();
            if (!PacientesRepositorio.Agregar(paciente))
                return Content(HttpStatusCode.Conflict, "Ya existe un paciente con ese DNI.");
            return CreatedAtRoute("ObtenerPacientePorDni", new { dni = paciente.Dni }, paciente);
        }

        /// <summary>Modifica un paciente por DNI.</summary>
        /// <param name="dni">DNI del paciente a modificar.</param>
        /// <param name="paciente">Datos nuevos (el DNI debe coincidir con el de la dirección).</param>
        [HttpPut, Route("{dni}")]
        public IHttpActionResult Put(string dni, [FromBody] Paciente paciente)
        {
            if (!DniValido(dni)) return DniInvalido();
            if (!ModelState.IsValid) return DatosInvalidos();
            if (paciente == null) return CuerpoFaltante();
            if (paciente.Dni != dni)
            {
                ModelState.AddModelError("paciente.Dni", "El DNI del cuerpo no coincide con el de la dirección.");
                return BadRequest(ModelState);
            }
            if (!PacientesRepositorio.Actualizar(dni, paciente)) return NotFound();
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>Elimina un paciente por DNI.</summary>
        /// <param name="dni">DNI del paciente.</param>
        [HttpDelete, Route("{dni}")]
        public IHttpActionResult Delete(string dni)
        {
            if (!DniValido(dni)) return DniInvalido();
            if (!PacientesRepositorio.Eliminar(dni)) return NotFound();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private static bool DniValido(string dni) => Regex.IsMatch(dni, ReglasPaciente.PatronDni);

        private IHttpActionResult DniInvalido()
        {
            ModelState.AddModelError("dni", ReglasPaciente.MensajeDni);
            return BadRequest(ModelState);
        }

        private IHttpActionResult CuerpoFaltante()
        {
            ModelState.AddModelError("paciente", "Falta el cuerpo de la solicitud o no es un objeto JSON válido.");
            return BadRequest(ModelState);
        }

        // Los errores al leer el JSON llegan como excepción, sin mensaje, y fuera de localhost Web API los oculta.
        // Se reemplazan por un mensaje claro (y se descartan los demás errores de ese campo, como "es obligatorio").
        private IHttpActionResult DatosInvalidos()
        {
            foreach (var campo in ModelState.Where(c => c.Value.Errors.Any(e => e.Exception != null)).ToList())
            {
                var excepcion = campo.Value.Errors.First(e => e.Exception != null).Exception;
                campo.Value.Errors.Clear();
                campo.Value.Errors.Add(MensajeDeLectura(campo.Key, excepcion));
            }
            return BadRequest(ModelState);
        }

        private static string MensajeDeLectura(string clave, Exception excepcion)
        {
            var campo = clave.Split('.').Last();
            if (campo.Equals("fechaNacimiento", StringComparison.OrdinalIgnoreCase))
                return "Debe ser una fecha válida con formato aaaa-mm-dd.";
            if (campo.Equals("obraSocial", StringComparison.OrdinalIgnoreCase))
                return "Debe ser uno de estos valores: " + string.Join(", ", Enum.GetNames(typeof(ObraSocial))) + ".";
            // JSON mal escrito: JsonReaderException, o "Unexpected end..." cuando el texto está cortado
            // (Newtonsoft no traduce sus mensajes, así que el texto es estable)
            var malEscrito = excepcion is Newtonsoft.Json.JsonReaderException || excepcion.Message.StartsWith("Unexpected end");
            if (malEscrito || campo.Equals("paciente", StringComparison.OrdinalIgnoreCase) || campo.Length == 0)
                return "El cuerpo no es un JSON válido.";
            return "Debe ser un texto.";
        }
    }
}

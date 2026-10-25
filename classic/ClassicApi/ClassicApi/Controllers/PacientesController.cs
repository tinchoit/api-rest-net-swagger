using System.Collections.Generic;
using System.Net;
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

        /// <summary>Obtiene un paciente por DNI.</summary>
        /// <param name="dni">DNI del paciente.</param>
        [HttpGet, Route("{dni}", Name = "ObtenerPacientePorDni")]
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult Get(string dni)
        {
            var paciente = PacientesRepositorio.ObtenerPorDni(dni);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        /// <summary>Crea un paciente (409 si el DNI ya existe).</summary>
        [HttpPost, Route("")]
        [ResponseType(typeof(Paciente))]
        public IHttpActionResult Post([FromBody] Paciente paciente)
        {
            // En Web API 2 hay que revisar el ModelState a mano (no existe [ApiController]).
            if (paciente == null || !ModelState.IsValid) return BadRequest(ModelState);
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
            if (paciente == null || !ModelState.IsValid) return BadRequest(ModelState);
            if (paciente.Dni != dni) return BadRequest("El DNI del cuerpo no coincide con el de la dirección.");
            if (!PacientesRepositorio.Actualizar(dni, paciente)) return NotFound();
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>Elimina un paciente por DNI.</summary>
        /// <param name="dni">DNI del paciente.</param>
        [HttpDelete, Route("{dni}")]
        public IHttpActionResult Delete(string dni)
        {
            if (!PacientesRepositorio.Eliminar(dni)) return NotFound();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using CoreApi.Models;
using CoreApi.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CoreApi.Endpoints;

// Endpoints de /api/pacientes: se registran desde Program.cs con app.MapPacientesEndpoints()
// Los tipos de retorno (Ok<T>, NotFound, Results<...>) le dicen a OpenAPI qué respuestas puede dar cada uno
public static class PacientesEndpoints
{
    public static IEndpointRouteBuilder MapPacientesEndpoints(this IEndpointRouteBuilder app)
    {
        var pacientes = app.MapGroup("/api/pacientes").WithTags("Pacientes");

        pacientes.MapGet("/", ObtenerTodos);
        pacientes.MapGet("/buscar", Buscar);     // /api/pacientes/buscar?nombre=Juan&apellido=Perez
        pacientes.MapGet("/{dni}", ObtenerPorDni);
        pacientes.MapPost("/", Crear);
        pacientes.MapPut("/{dni}", Actualizar);
        pacientes.MapDelete("/{dni}", Eliminar);

        return app;
    }

    // Listar todos
    static Ok<IReadOnlyList<Paciente>> ObtenerTodos(IPacienteRepositorio repo) => TypedResults.Ok(repo.ObtenerTodos());

    // Buscar por nombre y/o apellido
    static Ok<IReadOnlyList<Paciente>> Buscar(string? nombre, string? apellido, IPacienteRepositorio repo) =>
        TypedResults.Ok(repo.Buscar(nombre, apellido));

    // Ver uno por DNI (400 si el DNI no tiene formato válido)
    static Results<Ok<Paciente>, ValidationProblem, NotFound> ObtenerPorDni(
        [RegularExpression(ReglasPaciente.PatronDni)] string dni, IPacienteRepositorio repo)
    {
        if (!DniValido(dni)) return DniInvalido();
        return repo.ObtenerPorDni(dni) is { } paciente ? TypedResults.Ok(paciente) : TypedResults.NotFound();
    }

    // Crear (400 si algún dato no es válido, 409 si el DNI ya existe)
    static Results<Created<Paciente>, ValidationProblem, Conflict<string>> Crear(Paciente paciente, IPacienteRepositorio repo)
    {
        paciente = Normalizar(paciente);
        var errores = Validar(paciente);
        if (errores.Count > 0) return TypedResults.ValidationProblem(errores);
        if (!repo.Agregar(paciente)) return TypedResults.Conflict("Ya existe un paciente con ese DNI.");
        return TypedResults.Created($"/api/pacientes/{paciente.Dni}", paciente);
    }

    // Editar por DNI (el DNI del cuerpo debe coincidir con el de la dirección)
    static Results<NoContent, ValidationProblem, NotFound> Actualizar(
        [RegularExpression(ReglasPaciente.PatronDni)] string dni, Paciente paciente, IPacienteRepositorio repo)
    {
        if (!DniValido(dni)) return DniInvalido();
        paciente = Normalizar(paciente);
        var errores = Validar(paciente);
        if (errores.Count > 0) return TypedResults.ValidationProblem(errores);
        if (paciente.Dni != dni)
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                { ["dni"] = ["El DNI del cuerpo no coincide con el de la dirección."] });
        return repo.Actualizar(dni, paciente) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    // Eliminar por DNI
    static Results<NoContent, ValidationProblem, NotFound> Eliminar(
        [RegularExpression(ReglasPaciente.PatronDni)] string dni, IPacienteRepositorio repo)
    {
        if (!DniValido(dni)) return DniInvalido();
        return repo.Eliminar(dni) ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    static bool DniValido(string dni) => Regex.IsMatch(dni, ReglasPaciente.PatronDni);

    static ValidationProblem DniInvalido() =>
        TypedResults.ValidationProblem(new Dictionary<string, string[]>
            { ["dni"] = ["El DNI debe tener 7 u 8 dígitos, sin puntos ni letras."] });

    // Quita los espacios de los extremos: "  Ana  " pasa a ser "Ana"
    static Paciente Normalizar(Paciente p) => p with
    {
        Nombre = p.Nombre?.Trim()!,
        Apellido = p.Apellido?.Trim()!,
        Dni = p.Dni?.Trim()!,
        Telefono = p.Telefono?.Trim()!,
        Correo = p.Correo?.Trim()!
    };

    // Aplica los atributos de Paciente; las claves de los errores van en camelCase, como en el JSON
    static Dictionary<string, string[]> Validar(Paciente p)
    {
        var resultados = new List<ValidationResult>();
        Validator.TryValidateObject(p, new ValidationContext(p), resultados, validateAllProperties: true);
        return resultados
            .SelectMany(r => r.MemberNames.Select(campo => (Campo: JsonNamingPolicy.CamelCase.ConvertName(campo), Mensaje: r.ErrorMessage!)))
            .GroupBy(e => e.Campo)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Mensaje).ToArray());
    }
}

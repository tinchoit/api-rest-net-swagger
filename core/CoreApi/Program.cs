using CoreApi.Models;
using CoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios: el repositorio se registra una vez y se entrega donde se necesite
builder.Services.AddSingleton<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddOpenApi(opciones =>
{
    // Datos generales del contrato OpenAPI
    opciones.AddDocumentTransformer((documento, contexto, cancelacion) =>
    {
        documento.Info = new()
        {
            Title = "API de Pacientes - .NET",
            Version = "v1",
            Description = "API REST de pacientes de una clínica, documentada con OpenAPI."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapOpenApi();   // publica el contrato en /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "API de Pacientes v1"));   // interfaz en /swagger

// Cada endpoint se declara en pocas líneas, sin clases controladoras
var pacientes = app.MapGroup("/api/pacientes").WithTags("Pacientes");

// Listar todos
pacientes.MapGet("/", IResult (IPacienteRepositorio repo) => Results.Ok(repo.ObtenerTodos()));

// Buscar por nombre y/o apellido: /api/pacientes/buscar?nombre=Juan&apellido=Perez
pacientes.MapGet("/buscar", IResult (string? nombre, string? apellido, IPacienteRepositorio repo) =>
    Results.Ok(repo.Buscar(nombre, apellido)));

// Ver uno por DNI
pacientes.MapGet("/{dni}", IResult (string dni, IPacienteRepositorio repo) =>
    repo.ObtenerPorDni(dni) is { } paciente ? Results.Ok(paciente) : Results.NotFound());

// Crear (400 si falta algún dato, 409 si el DNI ya existe)
pacientes.MapPost("/", IResult (Paciente paciente, IPacienteRepositorio repo) =>
{
    var errores = Validar(paciente);
    if (errores.Count > 0) return Results.ValidationProblem(errores);
    if (!repo.Agregar(paciente)) return Results.Conflict("Ya existe un paciente con ese DNI.");
    return Results.Created($"/api/pacientes/{paciente.Dni}", paciente);
});

// Editar por DNI (el DNI del cuerpo debe coincidir con el de la dirección)
pacientes.MapPut("/{dni}", IResult (string dni, Paciente paciente, IPacienteRepositorio repo) =>
{
    var errores = Validar(paciente);
    if (errores.Count > 0) return Results.ValidationProblem(errores);
    if (paciente.Dni != dni) return Results.BadRequest("El DNI del cuerpo no coincide con el de la dirección.");
    return repo.Actualizar(dni, paciente) ? Results.NoContent() : Results.NotFound();
});

// Eliminar por DNI
pacientes.MapDelete("/{dni}", IResult (string dni, IPacienteRepositorio repo) =>
    repo.Eliminar(dni) ? Results.NoContent() : Results.NotFound());

app.Run();

// Todos los campos son obligatorios
static Dictionary<string, string[]> Validar(Paciente p)
{
    var errores = new Dictionary<string, string[]>();
    if (string.IsNullOrWhiteSpace(p.Nombre)) errores["nombre"] = ["El nombre es obligatorio."];
    if (string.IsNullOrWhiteSpace(p.Apellido)) errores["apellido"] = ["El apellido es obligatorio."];
    if (string.IsNullOrWhiteSpace(p.Dni)) errores["dni"] = ["El DNI es obligatorio."];
    if (p.FechaNacimiento == default) errores["fechaNacimiento"] = ["La fecha de nacimiento es obligatoria."];
    if (string.IsNullOrWhiteSpace(p.Telefono)) errores["telefono"] = ["El teléfono es obligatorio."];
    if (string.IsNullOrWhiteSpace(p.Correo)) errores["correo"] = ["El correo es obligatorio."];
    return errores;
}

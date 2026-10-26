using CoreApi.Configuracion;
using CoreApi.Endpoints;
using CoreApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Servicios: el repositorio se registra una vez y se entrega donde se necesite
builder.Services.AddSingleton<IPacienteRepositorio, PacienteRepositorio>();
builder.Services.AddErroresDeEntrada();

// Los enums viajan como texto ("Pami"); no se aceptan números (99) ni combinaciones ("Pami, Osde")
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new EnumComoTexto()));
builder.Services.AddOpenApi(opciones =>
{
    opciones.AddSchemaTransformer(EnumComoTexto.DescribirEnOpenApi);
    opciones.AddSchemaTransformer(CamposObligatorios.DescribirEnOpenApi);
    opciones.AddSchemaTransformer(EjemploPaciente.DescribirEnOpenApi);

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

app.UseErroresDeEntrada();   // 400 con detalle cuando el cuerpo no se puede leer

app.MapOpenApi();   // publica el contrato en /openapi/v1.json
app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "API de Pacientes v1"));   // interfaz en /swagger

// Endpoints de la API, definidos en la carpeta Endpoints
app.MapPacientesEndpoints();

app.Run();

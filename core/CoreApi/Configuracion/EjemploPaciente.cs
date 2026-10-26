using System.Text.Json.Nodes;
using CoreApi.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CoreApi.Configuracion;

public static class EjemploPaciente
{
    // Sin ejemplo propio, Swagger UI inventa valores a partir de los patrones del contrato,
    // y como no entiende \p{L} (cualquier letra) genera textos sin sentido: le damos uno fijo
    public static Task DescribirEnOpenApi(OpenApiSchema esquema, OpenApiSchemaTransformerContext contexto, CancellationToken cancelacion)
    {
        if (contexto.JsonTypeInfo.Type == typeof(Paciente))
        {
            esquema.Example = new JsonObject
            {
                ["nombre"] = "Ana",
                ["apellido"] = "López",
                ["dni"] = "35123456",
                ["fechaNacimiento"] = "1990-05-20",
                ["telefono"] = "1155551234",
                ["correo"] = "ana.lopez@mail.com",
                ["obraSocial"] = nameof(ObraSocial.Osde)
            };
        }
        return Task.CompletedTask;
    }
}

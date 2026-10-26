using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CoreApi.Configuracion;

public static class CamposObligatorios
{
    // Un campo con [Required] puede ser nullable en C# (DateOnly? para detectar si no se envió),
    // pero en el contrato no debe figurar como que acepta null
    public static Task DescribirEnOpenApi(OpenApiSchema esquema, OpenApiSchemaTransformerContext contexto, CancellationToken cancelacion)
    {
        var obligatorio = contexto.JsonPropertyInfo?.AttributeProvider?.IsDefined(typeof(RequiredAttribute), inherit: true) == true;
        if (obligatorio && esquema.Type is { } tipo)
            esquema.Type = tipo & ~JsonSchemaType.Null;
        return Task.CompletedTask;
    }
}

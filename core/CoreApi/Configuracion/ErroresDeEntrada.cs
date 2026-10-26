using System.Text.Json;
using CoreApi.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace CoreApi.Configuracion;

// Los errores al leer el cuerpo (JSON mal formado, tipo o valor inválido) por defecto devuelven 400 sin cuerpo.
// Esto los convierte en el mismo problem+json que usa la validación, indicando qué campo falló.
public static class ErroresDeEntrada
{
    public static IServiceCollection AddErroresDeEntrada(this IServiceCollection services) =>
        services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);   // así llegan como excepción al manejador

    public static WebApplication UseErroresDeEntrada(this WebApplication app)
    {
        app.UseExceptionHandler(manejador => manejador.Run(contexto =>
        {
            var error = contexto.Features.Get<IExceptionHandlerFeature>()?.Error;
            IResult respuesta = error switch
            {
                BadHttpRequestException { StatusCode: StatusCodes.Status400BadRequest } e => TypedResults.ValidationProblem(ErroresDe(e)),
                BadHttpRequestException e => TypedResults.Problem(statusCode: e.StatusCode),   // otros errores de la solicitud
                _ => TypedResults.Problem(statusCode: StatusCodes.Status500InternalServerError)
            };
            return respuesta.ExecuteAsync(contexto);
        }));
        return app;
    }

    static Dictionary<string, string[]> ErroresDe(BadHttpRequestException error)
    {
        // Sin JsonException: falta el cuerpo o es "null"
        if (error.InnerException is not JsonException json || json.Path is null or "$")
            return new() { ["cuerpo"] = ["Falta el cuerpo de la solicitud o no es un objeto JSON válido."] };

        // JSON mal escrito (llaves sin cerrar, comas de más): el error del lector viene envuelto en otro JsonException
        if (json.InnerException is JsonException)
            return new() { ["cuerpo"] = ["El cuerpo no es un JSON válido."] };

        var campo = JsonNamingPolicy.CamelCase.ConvertName(json.Path.TrimStart('$', '.'));
        var mensaje = campo switch
        {
            "fechaNacimiento" => "Debe ser una fecha válida con formato aaaa-mm-dd.",
            "obraSocial" => $"Debe ser uno de estos valores: {string.Join(", ", Enum.GetNames<ObraSocial>())}.",
            _ => "Debe ser un texto."
        };
        return new() { [campo] = [mensaje] };
    }
}

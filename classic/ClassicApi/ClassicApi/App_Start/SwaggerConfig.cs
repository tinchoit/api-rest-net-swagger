using System;
using System.Web.Http;
using WebActivatorEx;
using ClassicApi;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace ClassicApi
{
    // Paquete NuGet "Swashbuckle" 5.6.0. Genera Swagger 2.0 (la versión anterior de OpenAPI), no OpenAPI 3.
    public class SwaggerConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                {
                    c.DescribeAllEnumsAsStrings();   // muestra la obra social como texto
                    c.MapType<DateTime?>(() => new Swashbuckle.Swagger.Schema { type = "string", format = "date" });   // fecha sin hora (ver FechaSinHora)
                    c.SchemaFilter<EjemploPaciente>();   // ejemplo legible en lugar del que inventa Swagger UI
                    c.SingleApiVersion("v1", "API de Pacientes - .NET Framework 4.8");
                    // Requiere: Propiedades del proyecto > Compilar > "Archivo de documentación XML" = bin\ClassicApi.XML
                    c.IncludeXmlComments(String.Format(@"{0}\bin\ClassicApi.XML", AppDomain.CurrentDomain.BaseDirectory));
                })
                .EnableSwaggerUi();   // interfaz en /swagger
        }
    }
}


using System;
using System.Collections.Generic;
using ClassicApi.Models;
using Swashbuckle.Swagger;

namespace ClassicApi
{
    // Sin ejemplo propio, Swagger UI inventa valores a partir de los patrones del contrato,
    // y como no entiende \p{L} (cualquier letra) genera textos sin sentido: le damos uno fijo
    public class EjemploPaciente : ISchemaFilter
    {
        public void Apply(Schema esquema, SchemaRegistry registro, Type tipo)
        {
            if (tipo != typeof(Paciente)) return;

            esquema.example = new Dictionary<string, object>
            {
                ["Nombre"] = "Ana",
                ["Apellido"] = "López",
                ["Dni"] = "35123456",
                ["FechaNacimiento"] = "1990-05-20",
                ["Telefono"] = "1155551234",
                ["Correo"] = "ana.lopez@mail.com",
                ["ObraSocial"] = nameof(ObraSocial.Osde)
            };
        }
    }
}

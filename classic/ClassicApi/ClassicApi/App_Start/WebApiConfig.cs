using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ClassicApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de Web API

            // JSON: recorta los textos, acepta fechas solo como aaaa-mm-dd y enums solo por nombre (ver App_Start/ConversoresJson.cs)
            var json = config.Formatters.JsonFormatter.SerializerSettings;
            json.DateParseHandling = Newtonsoft.Json.DateParseHandling.None;   // las fechas llegan como texto a FechaSinHora
            json.Converters.Add(new RecortarTexto());
            json.Converters.Add(new FechaSinHora());
            json.Converters.Add(new EnumComoTexto());

            // Rutas de Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

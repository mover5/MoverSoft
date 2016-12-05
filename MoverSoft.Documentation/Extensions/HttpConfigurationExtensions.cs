using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace MoverSoft.Documentation.Extensions
{
    public static class HttpConfigurationExtensions
    {
        public static void BootstrapDocumentationRoute(
            this HttpConfiguration configuration,
            string routeName = "MoverSoftDocumentation",
            string routeTemplate = "api/documentation/swagger")
        {
            configuration.Routes.MapHttpRoute(
                name: routeName,
                routeTemplate: routeTemplate,
                defaults: new { controller = "MoverSoftDocumentation", action = "GetApiDocumentationSwagger" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
        }
    }
}

using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using MoverSoft.Documentation.Generators;
using MoverSoft.Documentation.Swagger;

namespace MoverSoft.Documentation.Controllers
{
    public class MoverSoftDocumentationController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public HttpResponseMessage GetApiDocumentationSwagger()
        {
            var info = SwaggerGenerator.GlobalApiInfo;
            if (info == null)
            {
                info = new SwaggerInfo
                {
                    Title = "The Api",
                    Version = "1.0.0.0"
                };
            }

            var generator = new SwaggerGenerator(
                configuration: GlobalConfiguration.Configuration,
                info: info);

            var swagger = generator.GenerateSwagger(
                hostName: this.Request.RequestUri.Host,
                urlSchemes: new string[] { this.Request.RequestUri.Scheme });

            return this.Request.CreateResponse(
                statusCode: HttpStatusCode.OK,
                value: swagger,
                configuration: GlobalConfiguration.Configuration);
        }
    }
}

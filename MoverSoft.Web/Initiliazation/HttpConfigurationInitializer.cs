namespace MoverSoft.Web.Initiliazation
{
    using System.Web.Http;
    using System.Web.Routing;
    using MoverSoft.Web.ErrorHandling;
    using MoverSoft.Web.Utilities;

    public class HttpConfigurationInitializer
    {
        public void Initialize(HttpConfiguration httpConfiguration)
        {
            this.ConfigureMediaTypeFormatters(httpConfiguration);
            this.ConfigureMessageHandlers(httpConfiguration);
            this.ConfigureFilters(httpConfiguration);
            this.RegisterRoutes(httpConfiguration);
        }

        public virtual void ConfigureMediaTypeFormatters(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Formatters.Clear();
            httpConfiguration.Formatters.Add(JsonMediaTypes.JsonMediaTypeFormatter);
        }

        public virtual void ConfigureMessageHandlers(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.MessageHandlers.Add(new ErrorResponseHandler(httpConfiguration));
        }

        public virtual void ConfigureFilters(HttpConfiguration httpConfiguration)
        {
            httpConfiguration.Filters.Add(new ErrorResponseFilter(httpConfiguration));
        }

        public virtual void RegisterRoutes(HttpConfiguration httpConfiguration)
        {
            RouteTable.Routes.MapPageRoute("Default", "{*wildcard}", "~/index.html");
        }
    }
}

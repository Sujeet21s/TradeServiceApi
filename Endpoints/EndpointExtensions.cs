using TradeServiceApi.Endpoints.ServiceEndpointHandler;

namespace TradeServiceApi.Endpoints
{
    public static class EndpointExtensions
    {
        public static WebApplication MapApplicationEndpoints(this WebApplication app)
        {
            app.MapServiceEndpoints();
            return app;
        }
    }
}

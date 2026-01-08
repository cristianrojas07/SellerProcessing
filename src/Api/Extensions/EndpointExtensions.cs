using Api.Endpoints;

namespace Api.Extensions;

public static class EndpointExtensions
{
    public static IApplicationBuilder MapApiEndpoints(this WebApplication app)
    {
        app.MapSellerEndpoints();

        return app;
    }
}
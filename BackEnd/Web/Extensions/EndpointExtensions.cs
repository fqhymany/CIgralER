using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace LawyerProject.Web.Extensions;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder RequireHeader(this RouteHandlerBuilder builder, string headerName)
    {
        builder.Add(endpointBuilder =>
        {
            var originalDelegate = endpointBuilder.RequestDelegate;
            endpointBuilder.RequestDelegate = async context =>
            {
                if (!context.Request.Headers.ContainsKey(headerName))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync($"Required header '{headerName}' is missing");
                    return;
                }

                if (originalDelegate != null)
                    await originalDelegate(context);
            };
        });

        return builder;
    }
}

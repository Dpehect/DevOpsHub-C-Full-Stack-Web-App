using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DevOpsHub.Api.OpenApi;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var allowsAnonymous = endpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (allowsAnonymous)
            return;

        var requiresAuthorization =
            endpointMetadata.OfType<AuthorizeAttribute>().Any()
            || context.MethodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Any() == true;

        if (!requiresAuthorization)
            return;

        operation.Security ??= [];

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        operation.Responses.TryAdd(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse { Description = "Unauthorized" });

        operation.Responses.TryAdd(
            StatusCodes.Status403Forbidden.ToString(),
            new OpenApiResponse { Description = "Forbidden" });
    }
}

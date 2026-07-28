using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DevOpsHub.Api.OpenApi;

public sealed class StandardResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Responses.TryAdd(
            StatusCodes.Status400BadRequest.ToString(),
            new OpenApiResponse
            {
                Description = "Validation failed."
            });

        operation.Responses.TryAdd(
            StatusCodes.Status429TooManyRequests.ToString(),
            new OpenApiResponse
            {
                Description = "Rate limit exceeded."
            });

        operation.Responses.TryAdd(
            StatusCodes.Status500InternalServerError.ToString(),
            new OpenApiResponse
            {
                Description = "Unexpected server error."
            });
    }
}

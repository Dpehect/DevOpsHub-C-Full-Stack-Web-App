using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DevOpsHub.Api.Filters;

public sealed class StrictRequestValidationFilter(
    IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments)
        {
            if (argument.Value is null)
                continue;

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argument.Value.GetType());

            if (serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument.Value);
            var result = await validator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted);

            if (result.IsValid)
                continue;

            context.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(result.ToDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Request validation failed.",
                    Instance = context.HttpContext.Request.Path
                });

            return;
        }

        await next();
    }
}

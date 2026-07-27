using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DevOpsHub.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ValidateRequestAttribute<T> : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var request = context.ActionArguments.Values.OfType<T>().FirstOrDefault();
        if (request is null)
        {
            await next();
            return;
        }

        var validator = context.HttpContext.RequestServices
            .GetService<IValidator<T>>();

        if (validator is null)
        {
            await next();
            return;
        }

        var result = await validator.ValidateAsync(
            request,
            context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(
                result.ToDictionary()));
            return;
        }

        await next();
    }
}

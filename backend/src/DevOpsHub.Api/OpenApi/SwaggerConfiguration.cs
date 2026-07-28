using Microsoft.OpenApi.Models;

namespace DevOpsHub.Api.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddEnterpriseSwagger(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "DevOpsHub API",
                Version = "v1",
                Description =
                    "Enterprise engineering operations API for workspaces, projects, " +
                    "pipelines, incidents, repositories, authentication and administration.",
                Contact = new OpenApiContact
                {
                    Name = "DevOpsHub Engineering"
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter: Bearer {JWT access token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            options.SupportNonNullableReferenceTypes();
        });

        return services;
    }

    public static IApplicationBuilder UseEnterpriseSwagger(
        this WebApplication app)
    {
        var enabled = app.Configuration.GetValue(
            "OpenApi:EnableSwaggerUi",
            app.Environment.IsDevelopment());

        if (!enabled)
            return app;

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/openapi.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api-docs";
            options.SwaggerEndpoint(
                "/api-docs/v1/openapi.json",
                "DevOpsHub API v1");
            options.DocumentTitle = "DevOpsHub API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnablePersistAuthorization();
            options.DefaultModelsExpandDepth(1);
        });

        return app;
    }
}

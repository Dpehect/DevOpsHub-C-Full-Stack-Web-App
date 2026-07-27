using System.Reflection;
using DevOpsHub.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace DevOpsHub.Tests;

public sealed class SecurityPolicyTests
{
    [Theory]
    [InlineData(typeof(AdminController))]
    [InlineData(typeof(AnalyticsController))]
    [InlineData(typeof(ProjectsController))]
    [InlineData(typeof(PipelinesController))]
    [InlineData(typeof(IncidentsController))]
    [InlineData(typeof(RepositoriesController))]
    [InlineData(typeof(DocumentationController))]
    [InlineData(typeof(NotificationsController))]
    [InlineData(typeof(ObservabilityController))]
    [InlineData(typeof(WorkspacesController))]
    public void Protected_controllers_require_authorization(Type controllerType)
    {
        var authorize = controllerType.GetCustomAttributes<AuthorizeAttribute>(inherit: true).ToArray();
        Assert.NotEmpty(authorize);
    }
}

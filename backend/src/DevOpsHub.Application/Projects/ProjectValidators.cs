using FluentValidation;
namespace DevOpsHub.Application.Projects;
public sealed class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(120); RuleFor(x => x.Key).NotEmpty().Matches("^[A-Za-z][A-Za-z0-9]{1,7}$"); RuleFor(x => x.Description).MaximumLength(1000); }
}
public sealed class CreateSprintValidator : AbstractValidator<CreateSprintRequest>
{
    public CreateSprintValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(100); RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate); RuleFor(x => x.Goal).MaximumLength(500); }
}
public sealed class CreateWorkItemValidator : AbstractValidator<CreateWorkItemRequest>
{
    public CreateWorkItemValidator() { RuleFor(x => x.Title).NotEmpty().MaximumLength(180); RuleFor(x => x.Description).MaximumLength(5000); RuleFor(x => x.StoryPoints).InclusiveBetween(0, 100); }
}

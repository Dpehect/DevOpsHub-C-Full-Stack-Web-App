using FluentValidation;

namespace DevOpsHub.Application.Workspaces;

public sealed class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceRequest>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
public sealed class InviteMemberValidator : AbstractValidator<InviteMemberRequest>
{
    public InviteMemberValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(160); }
}

using FluentValidation;

namespace DevOpsHub.Application.Common.Validation;

public sealed record PaginationRequest(int Page = 1, int PageSize = 25);

public sealed class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}

public sealed record SearchRequest(string? Query);

public sealed class SearchRequestValidator : AbstractValidator<SearchRequest>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .MaximumLength(200);
    }
}

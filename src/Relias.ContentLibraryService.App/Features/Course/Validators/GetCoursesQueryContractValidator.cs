using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Queries;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class GetCoursesQueryContractValidator : AbstractValidator<GetCoursesQuery.Contract>
{
    public GetCoursesQueryContractValidator()
    {
        RuleFor(x => x.OrganizationId)
            .GreaterThan(0).WithMessage("A valid organization id is required");
    }
}

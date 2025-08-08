using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Queries;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class GetCourseByIdQueryContractValidator : AbstractValidator<GetCourseByIdQuery.Contract>
{
    public GetCourseByIdQueryContractValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");
    }
}

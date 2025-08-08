using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Queries;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class GetFinalExamByCourseIdQueryContractValidator : AbstractValidator<GetFinalExamByCourseIdQuery.Contract>
{
    public GetFinalExamByCourseIdQueryContractValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty()
            .WithMessage("CourseId is required.");
    }
}

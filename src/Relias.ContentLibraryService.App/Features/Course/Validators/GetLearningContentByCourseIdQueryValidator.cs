using FluentValidation;
using Relias.ContentLibraryService.App.Features.Course.Queries;

namespace Relias.ContentLibraryService.App.Features.Course.Validators;

public class GetLearningContentByCourseIdQueryValidator : AbstractValidator<GetLearningContentByCourseIdQuery.Contract>
{
    public GetLearningContentByCourseIdQueryValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty().WithMessage("CourseId is required.");
    }
}

using FluentValidation;
using Relias.ContentLibraryService.Common.DTO;

namespace Relias.ContentLibraryService.App.Features
{
    public class HealthStatusDtoValidator : AbstractValidator<HealthStatusDto>
    {
        public HealthStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}

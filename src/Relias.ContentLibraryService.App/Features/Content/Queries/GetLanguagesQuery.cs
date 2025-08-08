using MediatR;
using Relias.ContentLibraryService.App.Features.Content.Dtos;
using Relias.ContentLibraryService.App.Interfaces.Content;
namespace Relias.ContentLibraryService.App.Features.Content.Queries;

public class GetLanguagesQuery
{
    public class Contract : IRequest<IEnumerable<LanguageDto>> { }

    public class Handler(ILanguageService languageService)
        : IRequestHandler<Contract, IEnumerable<LanguageDto>>
    {
        public async Task<IEnumerable<LanguageDto>> Handle(Contract contract, CancellationToken cancellationToken)
        {
            return await languageService.GetLanguagesAsync(cancellationToken);
        }
    }
}

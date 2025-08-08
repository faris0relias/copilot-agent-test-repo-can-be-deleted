using AutoMapper;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Helpers;
using System.Reflection;

namespace Relias.ContentLibraryService.App.Features.Course.Mappings;
public class FinalExamMappingConverter : ITypeConverter<Dictionary<string, object>, FinalExamDto>
{
    public FinalExamDto Convert(Dictionary<string, dynamic> dataValues, FinalExamDto destination, ResolutionContext context)
    {
        FinalExamDto convertedDto = new();
        foreach (var keyPair in dataValues.Where(prop => prop.Value is not null))
        {
            var propertyInfo = typeof(FinalExamDto).GetProperty(keyPair.Key, BindingFlags.IgnoreCase
            | BindingFlags.Public
            | BindingFlags.Instance);

            var value = JsonConverterHelper.ParsePropertyValue(keyPair.Key, keyPair.Value);
            propertyInfo?.SetValue(convertedDto, value);
        }

        return convertedDto;
    }
}

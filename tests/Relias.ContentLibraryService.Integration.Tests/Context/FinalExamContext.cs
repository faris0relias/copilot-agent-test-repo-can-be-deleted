using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.Integration.Tests.Context;

public class FinalExamContext
{
    public Guid CourseId { get; set; }
    public FinalExam? FinalExam { get; set; }
}

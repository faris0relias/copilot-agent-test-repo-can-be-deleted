
namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{
    internal class Course
    {
        public Guid CourseId { get; set; }
        public int OrganizationId { get; set; }
        public Guid ContentId { get; set; }
        public required string ContentCode { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? BriefDescription { get; set; }
        public int StatusId { get; set; }
        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
}
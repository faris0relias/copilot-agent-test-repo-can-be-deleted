namespace Relias.ContentLibraryService.Domain.PolicyTag
{
    public class Tag
    {
        public int TagId { get; set; } 
        public string Value { get; set; }
        public ICollection<Policy.Policy> Policies { get; set; } = new List<Policy.Policy>();
    }
}
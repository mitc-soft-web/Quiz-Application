using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Course : BaseEntity
    {
        public string? CourseName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public List<Language> Languages { get; set; } = new List<Language>();
    }
}

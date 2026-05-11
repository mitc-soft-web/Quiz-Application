
namespace Quiz_Application.Models.DTO
{
    public class CourseDTO
    {
        public Guid Id { get; set; }
        public string? CourseName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public List<Language> Languages { get; set; } = new List<Language>();
        public List<Controllers.LanguageDTO> Topics { get; internal set; } = new();
    }
}

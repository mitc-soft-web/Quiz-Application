namespace Quiz_Application.Models.DTO
{
    public class LanguageDTO
    {
        public Guid Id { get; set; }
        public string? LanguageName { get; set; }
        public Guid CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? Description { get;  set; }
        public string? Category { get;  set; }
    }
}

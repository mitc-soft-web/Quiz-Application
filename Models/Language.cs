using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Language : BaseEntity
    {
        public string? LanguageName { get; set; }
        public string? CourseName { get; set; }
        public Guid CourseId { get; set; }
        public virtual Course? Course { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public string Description { get; internal set; } = string.Empty;
    }
}

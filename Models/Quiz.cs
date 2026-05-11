using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Quiz : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string? Level { get; set; }
        public int? ExternalCourseId { get; set; }
        public int? TotalQuestions { get; set; }
        public int? Score { get; set; }
        public Result? Result { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public Guid LanguageId { get; set; }
        public virtual Language? Language { get; set; }
    }
}

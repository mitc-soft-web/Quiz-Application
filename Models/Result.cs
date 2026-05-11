using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Result : BaseEntity
    {
        public Guid QuizId { get; set; }
        public virtual Quiz? Quiz { get; set; }
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Score { get; set; }
        public DateTime CompletedDate { get; set; } = DateTime.UtcNow;
        public string? Remarks { get; set; }
    }
}

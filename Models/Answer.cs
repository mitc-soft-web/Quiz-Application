using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Answer : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public virtual Question? Question { get; set; }
        public string? SelectedOption { get; set; }
        public string? AnswerText { get; set; }
        public bool IsCorrect { get; set; }
        public Guid? QuizId { get; set; }
    }
}

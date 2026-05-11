using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class Question : BaseEntity
    {
        public string? QuestionText { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectOption { get; set; }
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public Guid LanguageId { get; set; }
        public Language? Language { get; set; }
        public string? Difficulty { get; set; }
        public  ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}

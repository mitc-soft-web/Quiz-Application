namespace Quiz_Application.Models.DTO.Question
{
    public class QuestionDTO
    {
        public Guid Id { get; set; }
        public string? QuestionText { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? Answers { get; set; }
        public string Text { get; set; } = string.Empty;
        public Guid LanguageId { get; set; }
        public string? Difficulty { get; set; }
        public string? Category { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public string? UserSelection { get; set; }
        public string? CorrectAnswer { get; set; }
        public Guid QuizId { get;  set; }
        public string? CorrectOption { get;  set; }
    }
}

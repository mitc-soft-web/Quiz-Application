namespace Quiz_Application.Models.DTO
{
    public class ResultDTO
    {
        public Guid QuizId { get; set; }
        public string? UserName { get; set; }
        public string? CourseName { get; set; }
        public string? LanguageName { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public string Level { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Score { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public Guid Id { get; set; }
    }
}

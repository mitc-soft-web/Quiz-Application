namespace Quiz_Application.Models.DTO
{
    public class CourseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string ProficiencyLevel { get; set; } = string.Empty;
        public double LatestScore { get; set; }
        public DateTime LastAssessed { get; set; }
    }

    public class SuggestionViewModel
    {
        public string LanguageName { get; set; } = string.Empty;

        public string SuggestionText { get; set; } = string.Empty;
        public string RESourceLink { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }

    public class AnswerViewModel
    {
        public Guid Id { get; set; }
        public Guid SkillId { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public double Score { get; set; }
        public string ProficiencyLevel { get; set; } = string.Empty;
        public DateTime TakenOn { get; set; }
        public int TotalQuestions { get; set; }
        public int NoOfCorrectAnswers { get; set; }
        public int NoOfWrongAnswers { get; set; }
        public int RetakeCount { get; set; }
        public List<Answer> WrongAnswers { get; set; } = new();
    }
}

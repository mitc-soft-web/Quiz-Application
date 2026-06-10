namespace Quiz_Application.Models.DTO
{
    public class QuizSetupViewModel
    {
        public Guid LanguageId { get; set; }
        public string LanguageName { get; set; } = "Unknown Topic";
        public List<string> Subtopics { get; set; } = new();
    }
}

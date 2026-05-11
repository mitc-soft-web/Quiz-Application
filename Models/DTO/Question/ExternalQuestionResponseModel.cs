namespace Quiz_Application.Models.DTO.Question
{
    public class ExternalQuestionResponseModel
    {
        public Guid Id { get; set; }
        public string? QuestionText { get; set; }
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Category { get; set; }
        public List<string> Answers { get; set; } = new List<string>();
    }
}

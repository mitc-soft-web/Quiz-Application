namespace Quiz_Application.Models.DTO
{
    public class AnswerDTO
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string? SelectedOption { get; set; }
        public bool IsCorrect { get; set; }
    }
}

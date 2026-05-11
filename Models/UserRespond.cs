namespace Quiz_Application.Models
{
    public class UserResponse
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public string? SelectedOption { get; set; } 
        public DateTime Timestamp { get; set; }
    }
}

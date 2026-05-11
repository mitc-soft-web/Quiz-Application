namespace Quiz_Application.Models
{
    public class Suggestion
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid LanguageId { get; set; } 
        public Guid CourseId { get; set; }   
        public Guid ResultId { get; set; }

        public User? User { get; set; }
        public Language? Language { get; set; }
        public Course? Course { get; set; }
        public Result? Result { get; set; }
        public string Suggestions { get; set; } = string.Empty;
        public string ResourceLink { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }


}

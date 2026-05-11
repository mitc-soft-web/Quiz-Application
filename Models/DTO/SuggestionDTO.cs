namespace Quiz_Application.Models.DTO
{
    namespace Quiz_Application.Models.DTO
    {
        public class SuggestionDto
        {
            public Guid Id { get; set; }
            public string Suggestions { get; set; } = string.Empty;
            public string ResourseLienk { get; set; } = string.Empty;

            public DateTime SavedAt { get; set; }
        }
    }

}

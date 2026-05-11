using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class ExternalQuizAPI : BaseEntity
    {
        public string? ProviderName { get; set; }
        public string? BaseUrl { get; set; }
        public string? ApiKey { get; set; }
        public bool IsActive { get; set; }
    }

}

using Microsoft.AspNetCore.Identity;
using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Models
{
    public class User : BaseEntity
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public virtual Course? Course { get; set; }
        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}

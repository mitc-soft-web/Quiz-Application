namespace Quiz_Application.Models.DTO.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? PreferredCourse { get; set; }
    }
}

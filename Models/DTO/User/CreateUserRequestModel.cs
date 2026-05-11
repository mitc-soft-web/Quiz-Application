namespace Quiz_Application.Models.DTO.User
{
    public class CreateUserRequestModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public Guid? CourseId { get; set; }
    }
}

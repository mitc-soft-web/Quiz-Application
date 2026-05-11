using System.ComponentModel.DataAnnotations;

namespace Quiz_Application.Models.DTO.User
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Email is required for authentication")]
        [EmailAddress]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Security credential is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

    public class LoginResponseModel
    {
        public bool IsSuccess { get; set; }
        public bool Status { get; set; } 
        public string? Message { get; set; }
        public string? Token { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }

        // Detailed User Profile
        public UserDTO? User { get; set; }
    }
}

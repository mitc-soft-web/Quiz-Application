using FluentValidation;

namespace Quiz_Application.Models.DTO.User
{
    public class CreateUserValidation : AbstractValidator<CreateUserRequestModel>
    {
        public CreateUserValidation()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is required.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            RuleFor(x => x.ConfirmPassword).NotEmpty().Equal(x => x.Password).WithMessage("Passwords do not match.");
        }
    }
}

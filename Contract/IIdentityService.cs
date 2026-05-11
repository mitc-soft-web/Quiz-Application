using Quiz_Application.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Quiz_Application.Contract
{
    public interface IIdentityService 
    {
        string GetUserIdentity();

        string GenerateToken(User user, IEnumerable<string> roles);
        public IEnumerable<Claim> ValidateToken(string jwtToken);
        JwtSecurityToken GetClaims(string token);
        string GetClaimValue(string type);
        string GenerateSalt();
        public string GetPasswordHash(string password, string? salt = null);
        Task<User> FindByNameAsync(string userName);
        Task<User> FindUserAsync(string userName);
        public Task<User> GetLoggedInUser();
        bool VerifyPassword(string? password, string? passwordHash);
    }
}

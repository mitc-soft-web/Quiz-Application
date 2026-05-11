using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Quiz_Application.Contract;
using Org.BouncyCastle.Crypto.Generators;

namespace Quiz_Application.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userRepository;

        public IdentityService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IPasswordHasher<User> passwordHasher,
            IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _passwordHasher = passwordHasher ?? throw new ArgumentException(nameof(passwordHasher));
            _userRepository = userRepository;
        }

        //public bool CheckPasswordAsync(User user, string password)
        //{
        //    var hashPassword = HashPasswordAsync(password);
        //    if (user.PasswordHash == hashPassword)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        public string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            using var crypto = RandomNumberGenerator.Create();
            byte[] data = new byte[size];
            crypto.GetBytes(data);
            /*using (var crypto = new RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }*/
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public async Task<User> FindByNameAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            var user = await _userRepository.GetUserByUsernameAsync(userName);
            return user ?? throw new KeyNotFoundException("User not found.");
        }

        public async Task<User> FindUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }
            //var user = await _gateway.GetUserAsync(userName);
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user ?? throw new KeyNotFoundException("User not found.");
        }

        public string GenerateSalt()
        {
            using var crypto = RandomNumberGenerator.Create();
            byte[] buffer = new byte[10];
            crypto.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public string GenerateToken(User user, IEnumerable<string> roles)
        {
            var tokenKey = _configuration.GetSection("JwtTokenSettings:TokenKey").Value
                ?? "development-token-key-change-this-value";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            IList<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var token = new JwtSecurityToken("", "", claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetSection("JwtTokenSettings:TokenExpiryPeriod").Value ?? "60")),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken GetClaims(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (token.StartsWith("B"))
                {
                    token = token.Split(" ")[1];
                }
                var handler = new JwtSecurityTokenHandler();

                var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

                return decodedToken ?? throw new SecurityTokenException("Invalid token.");
            }
            Console.WriteLine(token);
            throw new SecurityTokenException("Token is empty.");
        }

        public string GetClaimValue(string type)
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value ?? string.Empty;
        }

        public string GetPasswordHash(string password, string? salt = null)
        {
            if (string.IsNullOrEmpty(salt))
            {
                return _passwordHasher.HashPassword(new User(), password);
            }
            return _passwordHasher.HashPassword(new User(), $"{password}{salt}");
        }

        public string GetUserIdentity()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;
        }

        //private string HashPasswordAsync(string password)
        //{
        //    using (var md5Hash = MD5.Create())
        //    {
        //        var sourceBytes = Encoding.UTF8.GetBytes(password);
        //        var hashBytes = md5Hash.ComputeHash(sourceBytes);
        //        var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        //        return hash.ToLower();
        //    }
        //}



        public IEnumerable<Claim> ValidateToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = _configuration.GetSection("JwtTokenSettings:TokenKey").Value
                ?? "development-token-key-change-this-value";
            var key = Encoding.ASCII.GetBytes(tokenKey);

            try
            {
                // Set the validation parameters for the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key.ToArray()),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = _configuration.GetSection("JwtTokenSettings:TokenIssuer").Value,
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the token and extract the claims
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                var claims = ((JwtSecurityToken)validatedToken).Claims;

                // Return the claims
                return claims;
            }
            catch (Exception)
            {
                return Enumerable.Empty<Claim>();
            }
        }

        public async Task<User> GetLoggedInUser()
        {
            // Get the current HttpContext
            var httpContext = _httpContextAccessor.HttpContext;

            // Check if a user is authenticated
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Retrieve the user's unique identifier (e.g., user ID) from claims
                var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new BadHttpRequestException("Unable to get logged in user");
                }

                var user = await _userRepository.GetUserByEmailAsync(email);

                return user ?? throw new BadHttpRequestException("Unable to get logged in user");



            }

            // If no user is authenticated, return null
            throw new BadHttpRequestException("Unable to get logged in user");
        }


        public bool VerifyPassword(string? password, string? passwordHash)
         {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
              return false;
        
            var hasher = new PasswordHasher<object>();

             // VerifyHashedPassword returns a PasswordVerificationResult enum
            var result = hasher.VerifyHashedPassword(new object(), passwordHash, password);

            return result == PasswordVerificationResult.Success;
        }

    }
}

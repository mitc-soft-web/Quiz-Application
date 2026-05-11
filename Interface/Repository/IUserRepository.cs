using System;
using System.Threading.Tasks;
using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task SaveAsync();
        Task<int> GetUsersCountAsync(CancellationToken cancellationToken);
    }
}


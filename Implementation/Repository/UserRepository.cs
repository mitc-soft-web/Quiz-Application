using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;
using System;
using System.Threading.Tasks;

namespace Quiz_Application.Implementation.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly QuizContext _quizContext;

        public UserRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task AddUserAsync(User user)
        {
            await _quizContext.Users.AddAsync(user);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _quizContext.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _quizContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _quizContext.Users
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<int> GetUsersCountAsync(CancellationToken cancellationToken)
        {
            return await _quizContext.Users.CountAsync(cancellationToken);
        }

        public async Task SaveAsync()
        {
            await _quizContext.SaveChangesAsync();
        }

        public async Task<bool> UserExistsByEmailAsync(string email)
        {
            return await _quizContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByUsernameAsync(string username)
        {
            return await _quizContext.Users.AnyAsync(u => u.UserName == username);
        }
    }
}

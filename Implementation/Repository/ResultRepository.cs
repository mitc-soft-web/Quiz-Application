using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;

namespace Quiz_Application.Implementation.Repository
{
    public class ResultRepository : IResultRepository
    {
        private readonly QuizContext _quizContext;

        public ResultRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task AddResultAsync(Result result)
        {
            await _quizContext.Results.AddAsync(result);
        }

        public async Task<IEnumerable<Result>> GetAllResultsAsync()
        {
            return await _quizContext.Results
                .Include(r => r.User)
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Language)
                .OrderByDescending(r => r.CompletedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Result?> GetResultByQuizIdAsync(Guid quizId)
        {
            return await _quizContext.Results
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Language)
                .FirstOrDefaultAsync(r => r.QuizId == quizId);
        }

        public async Task<IEnumerable<Result>> GetResultsByUserIdAsync(Guid userId)
        {
            return await _quizContext.Results
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Language)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CompletedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _quizContext.SaveChangesAsync();
        }
    }
}

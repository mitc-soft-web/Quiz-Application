using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Implementation.Service;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;
using Quiz_Application.Models.BaseEntities;

namespace Quiz_Application.Implementation.Repository
{
    public class QuizRepository : IQuizRepository
    {
        private readonly QuizContext _quizContext;

        public QuizRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task<Quiz> CreateQuiz(Quiz quiz)
        {
            await _quizContext.Quizzes.AddAsync(quiz);
            await _quizContext.SaveChangesAsync();
            return quiz;
        }

        public async Task<IEnumerable<Quiz>> GetAllQuizzes()
        {
            return await _quizContext.Quizzes
                .Include(q => q.Language)
                .OrderByDescending(q => q.CreatedDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Quiz?> GetQuizById(Guid quizId)
        {
            return await _quizContext.Quizzes
                .Include(q => q.Language)
                .Include(q => q.Questions)
                    .ThenInclude(ques => ques.Answers)
                .FirstOrDefaultAsync(q => q.Id == quizId);
        }

        public async Task<List<Quiz>> GetQuizzesByUserId(Guid userId)
        {
            return await _quizContext.Quizzes
                .Include(q => q.Language)
                .Include(q => q.Result)
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Question>> GetQuestionsByQuizId(Guid quizId)
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }

        public async Task SaveResult(Result result)
        {
            await _quizContext.Results.AddAsync(result);
            await _quizContext.SaveChangesAsync();
        }

        public async Task<Result?> GetResultByQuizId(Guid quizId)
        {
            return await _quizContext.Results
                .FirstOrDefaultAsync(r => r.QuizId == quizId);
        }

        public async Task SubmitAnswer(Guid questionId, string selectedAnswer)
        {
            await Task.CompletedTask;
        }

        public async Task<Result?> GetResultByQuiz()
        {
            return await _quizContext.Results
                .OrderByDescending(r => r.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task<Result?> GetResultById(Guid quizResultId)
        {
            return await _quizContext.Results
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Language)
                .Include(r => r.Quiz!)
                    .ThenInclude(q => q.Questions)
                .FirstOrDefaultAsync(r => r.Id == quizResultId);
        }

    }
}


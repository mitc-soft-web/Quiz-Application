using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;

namespace Quiz_Application.Implementation.Repository
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly QuizContext _quizContext;

        public QuestionRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task AddQuestionAsync(Question question)
        {
            await _quizContext.Questions.AddAsync(question);
        }

        public async Task<IEnumerable<Question>> AddQuestionsAsync(IEnumerable<Question> questions)
        {
            await _quizContext.Questions.AddRangeAsync(questions);
            return questions;
        }

        public async Task<IEnumerable<Question>> GetAllQuestionsAsync()
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .ToListAsync();
        }

        public async Task<Question?> GetQuestionByIdAsync(Guid questionId)
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<IEnumerable<Question>> GetQuestionsByQuizIdAsync(Guid quizId)
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count)
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .OrderBy(q => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        public async Task SaveAsync()
        {
            await _quizContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Question question)
        {
            _quizContext.Questions.Remove(question);
            await Task.CompletedTask; 
        }

        public async Task<IEnumerable<Question>> GetFilteredQuestionsAsync(Guid languageId, string level, int count)
        {
            return await _quizContext.Questions
                .Include(q => q.Answers)
                .Where(q => q.LanguageId == languageId && q.Difficulty == level)
                .OrderBy(r => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }
    }
}

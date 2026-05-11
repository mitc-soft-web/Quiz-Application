using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;


namespace Quiz_Application.Implementation.Repository
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly QuizContext _quizContext;

        public AnswerRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task<IEnumerable<Answer>> GetAnswersByQuestionIdAsync(Guid questionId)
        {
            return await _quizContext.Answers
                .Where(a => a.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<Answer?> GetCorrectAnswerAsync(Guid questionId)
        {
            return await _quizContext.Answers
                .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.IsCorrect);
        }

        public async Task SaveUserAnswerAsync(Guid questionId, string selectedAnswer)
        {
            var response = new UserResponse
            {
                QuestionId = questionId,
                SelectedOption = selectedAnswer,
                Timestamp = DateTime.UtcNow
            };

            await _quizContext.UserResponses.AddAsync(response);
        }

        public async Task SaveAsync()
        {
            await _quizContext.SaveChangesAsync();
        }

        public async Task AddRangeAsync(object entities)
        {
            if (entities is IEnumerable<Answer> answers)
            {
                await _quizContext.Answers.AddRangeAsync(answers);
            }
        }
    }
}

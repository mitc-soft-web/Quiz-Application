
using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface IAnswerRepository
    {
        Task<IEnumerable<Answer>> GetAnswersByQuestionIdAsync(Guid questionId);
        Task<Answer?> GetCorrectAnswerAsync(Guid questionId);
        Task SaveUserAnswerAsync(Guid questionId, string selectedAnswer);
        Task SaveAsync();
        Task AddRangeAsync(object entities);
    }
}

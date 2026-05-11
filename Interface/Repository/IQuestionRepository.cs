using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface IQuestionRepository
    {
        Task<IEnumerable<Question>> AddQuestionsAsync(IEnumerable<Question> questions);
        Task AddQuestionAsync(Question question);
        Task<IEnumerable<Question>> GetQuestionsByQuizIdAsync(Guid quizId);
        Task<Question?> GetQuestionByIdAsync(Guid questionId);
        Task<IEnumerable<Question>> GetAllQuestionsAsync();
        Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count);
        Task<IEnumerable<string>> GetPreviousQuestionTextsAsync(Guid userId, Guid languageId);
        Task SaveAsync();
        Task DeleteAsync(Question question);
    }
}

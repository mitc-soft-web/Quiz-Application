using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface IQuizRepository
    {
        Task<Quiz> CreateQuiz(Quiz quiz);
        Task<Quiz?> GetQuizById(Guid quizId);
        Task<List<Quiz>> GetQuizzesByUserId(Guid userId);
        Task SaveResult(Result result);
        Task<List<Question>> GetQuestionsByQuizId(Guid quizId);
        Task SubmitAnswer(Guid questionId, string selectedAnswer);
        Task<IEnumerable<Quiz>> GetAllQuizzes();
        Task<Result?> GetResultById(Guid quizResultId);
        Task<Result?> GetResultByQuizId(Guid quizId);
    }
}

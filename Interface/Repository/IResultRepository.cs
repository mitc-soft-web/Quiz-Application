using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface IResultRepository
    {
        Task AddResultAsync(Result result);
        Task<IEnumerable<Result>> GetAllResultsAsync();
        Task<Result?> GetResultByQuizIdAsync(Guid quizId);
        Task<IEnumerable<Result>> GetResultsByUserIdAsync(Guid userId);
        Task SaveAsync();
    }
}

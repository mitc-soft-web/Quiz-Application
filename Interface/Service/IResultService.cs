using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Interface.Service
{
    public interface IResultService
    {
        Task<ResultDTO> CalculateResultAsync(Guid quizId, Dictionary<Guid, string> userAnswers, CancellationToken cancellationToken);
        Task<ResultDTO?> GetResultByQuizIdAsync(Guid quizId, CancellationToken cancellationToken);
        Task<IEnumerable<ResultDTO>> GetResultsByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<IEnumerable<ResultDTO>> GetAllResultsAsync(CancellationToken cancellationToken );
    }
}

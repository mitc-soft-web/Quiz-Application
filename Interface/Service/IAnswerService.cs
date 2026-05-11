using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Interface.Service
{
    public interface IAnswerService
    {
        Task<IEnumerable<AnswerDTO>> GetAnswersByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
        Task<AnswerDTO?> GetCorrectAnswerAsync(Guid questionId, CancellationToken cancellationToken = default);
        Task<bool> ValidateAnswerAsync(Guid questionId, Guid answerId, CancellationToken cancellationToken = default);
        Task<bool> SaveAnswersAsync(Guid questionId, List<AnswerDTO> answers, CancellationToken cancellationToken = default);
        Task<bool> DeleteAnswerAsync(Guid answerId, CancellationToken cancellationToken = default);
    }
}


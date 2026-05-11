using Quiz_Application.Interface.Repository;
using Quiz_Application.Interface.Service;
using Quiz_Application.Models;
using Quiz_Application.Models.DTO;
using Microsoft.Extensions.Logging;
using Quiz_Application.Interfaces.Repositories;

namespace Quiz_Application.Implementation.Service
{
    public class ResultService : IResultService
    {
        private readonly IResultRepository _resultRepository;
        private readonly IAnswerRepository _answerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResultService> _logger;

        public ResultService(
            IResultRepository resultRepository,
            IAnswerRepository answerRepository,
            IUnitOfWork unitOfWork,
            ILogger<ResultService> logger)
        {
            _resultRepository = resultRepository;
            _answerRepository = answerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResultDTO> CalculateResultAsync(Guid quizId, Dictionary<Guid, string> userAnswers, CancellationToken cancellationToken)
        {
            int totalQuestions = userAnswers.Count;
            int correctAnswersCount = 0;

            try
            {
                foreach (var entry in userAnswers)
                {
                    var questionId = entry.Key;
                    var selectedOption = entry.Value;

                    var correctAnswer = await _answerRepository.GetCorrectAnswerAsync(questionId);

                    if (correctAnswer != null && string.Equals(correctAnswer.SelectedOption?.Trim(), selectedOption?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        correctAnswersCount++;
                    }

                    await _answerRepository.SaveUserAnswerAsync(questionId, selectedOption ?? string.Empty);
                }

                var resultEntity = new Result
                {
                    Id = Guid.NewGuid(),
                    QuizId = quizId,
                    TotalQuestions = totalQuestions,
                    CorrectAnswers = correctAnswersCount,
                    Score = totalQuestions > 0 ? (correctAnswersCount * 100) / totalQuestions : 0,
                    CompletedDate = DateTime.UtcNow
                };

                await _resultRepository.AddResultAsync(resultEntity);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return MapToDto(resultEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Engine Failure: Simulation results for Quiz {QuizId} could not be synchronized.", quizId);
                throw;
            }
        }

        public async Task<ResultDTO?> GetResultByQuizIdAsync(Guid quizId, CancellationToken cancellationToken)
        {
            var result = await _resultRepository.GetResultByQuizIdAsync(quizId);
            return result == null ? null : MapToDto(result);
        }

        public async Task<IEnumerable<ResultDTO>> GetResultsByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var results = await _resultRepository.GetResultsByUserIdAsync(userId);
            return results.Select(MapToDto).OrderByDescending(r => r.CompletedAt);
        }

        public async Task<IEnumerable<ResultDTO>> GetAllResultsAsync(CancellationToken cancellationToken)
        {
            var results = await _resultRepository.GetAllResultsAsync();
            return results.Select(MapToDto);
        }

        private static ResultDTO MapToDto(Result result) => new ResultDTO
        {
            Id = result.Id,
            QuizId = result.QuizId,
            Score = result.Score,
            TotalQuestions = result.TotalQuestions,
            CorrectAnswers = result.CorrectAnswers,
            CompletedAt = result.CompletedDate,
        };
    }
}

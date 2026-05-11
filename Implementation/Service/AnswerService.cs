using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Interface.Service;
using Quiz_Application.Interfaces.Repositories;
using Quiz_Application.Models;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Implementation.Service
{
    public class AnswerService : IAnswerService
    {
        private readonly IAnswerRepository _answerRepository;
        private readonly IUnitOfWork _unitOfWork; 
        private readonly ILogger<AnswerService> _logger;

        public AnswerService(
            IAnswerRepository answerRepository, 
            IUnitOfWork unitOfWork,
            ILogger<AnswerService> logger)
        {
            _answerRepository = answerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<AnswerDTO>> GetAnswersByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken)
        {
            var answers = await _answerRepository.GetAnswersByQuestionIdAsync(questionId);

            return answers.Select(a => new AnswerDTO
            {
                Id = a.Id,
                SelectedOption = a.SelectedOption, 
                IsCorrect = a.IsCorrect,
                QuestionId = a.QuestionId
            });
        }

        public async Task<AnswerDTO?> GetCorrectAnswerAsync(Guid questionId, CancellationToken cancellationToken)
        {
            var answer = await _answerRepository.GetCorrectAnswerAsync(questionId);

            if (answer == null) return null;

            return new AnswerDTO
            {
                Id = answer.Id,
                SelectedOption = answer.SelectedOption,
                IsCorrect = answer.IsCorrect,
                QuestionId = answer.QuestionId
            };
        }

        public async Task<bool> ValidateAnswerAsync(Guid questionId, Guid answerId, CancellationToken cancellationToken)
        {
            var options = await _answerRepository.GetAnswersByQuestionIdAsync(questionId);
            var pickedOption = options.FirstOrDefault(a => a.Id == answerId);

            if (pickedOption == null)
            {
                _logger.LogWarning("Simulation Validation: Option {AnswerId} not found for Question {QuestionId}", answerId, questionId);
                return false;
            }

            await _answerRepository.SaveUserAnswerAsync(questionId, pickedOption.SelectedOption ?? string.Empty);
            
            return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0 && pickedOption.IsCorrect;
        }

        public async Task<bool> SaveAnswersAsync(Guid questionId, List<AnswerDTO> answers, CancellationToken cancellationToken)
        {
            try 
            {
                var entities = answers.Select(dto => new Answer 
                {
                    Id = Guid.NewGuid(),
                    QuestionId = questionId,
                    SelectedOption = dto.SelectedOption,
                    IsCorrect = dto.IsCorrect
                }).ToList();

                await _answerRepository.AddRangeAsync(entities);
                
                return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Module Sync Failure: Could not save answers for Question {QuestionId}", questionId);
                return false;
            }
        }

        public async Task<bool> DeleteAnswerAsync(Guid answerId, CancellationToken cancellationToken)
        {
            try
            {
                var stub = new Answer { Id = answerId };

                return await _unitOfWork.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Purge Warning: Answer {AnswerId} was already gone.", answerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Engine Error: Failed to remove Answer module {AnswerId}", answerId);
                return false;
            }
        }


    }
}

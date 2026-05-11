using Quiz_Application.Models.DTO.Quiz_Application.Models.DTO;

namespace Quiz_Application.Interface.Service
{
    public interface ISuggestionService
    {
        Task<List<SuggestionDto>> GetSuggestionsAsync(Guid QuizResultId);
        Task SaveSuggestionAsync(Guid quizResultId, List<SuggestionDto> suggestionDto);
        public List<SuggestionDto> GetSuggestionsFromCache(Guid QuizResultId);
    }
}

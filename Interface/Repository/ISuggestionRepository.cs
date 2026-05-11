using Quiz_Application.Models;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Interface.Repository
{
    public interface ISuggestionRepository
    {
        Task AddAsync(Suggestion suggestion);
        Task<List<SuggestionVm>> SuggestionHightlights(Guid userId);
        Task<List<SuggestionViewModel>> UserSuggestions(Guid userId);
        Task<List<Suggestion>> GetSuggestionsForQuizResultAsync(Guid ResultId);
    }
}

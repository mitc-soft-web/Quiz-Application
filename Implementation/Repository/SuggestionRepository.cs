using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Implementation.Repository
{
    public class SuggestionRepository : ISuggestionRepository
    {
        private readonly QuizContext _quizcontext;

        public SuggestionRepository(QuizContext context)
        {
            _quizcontext = context;
        }

        public async Task AddAsync(Suggestion suggestion)
        {
            await _quizcontext.Suggestions.AddAsync(suggestion);
            await _quizcontext.SaveChangesAsync();
        }

        public async Task<List<SuggestionVm>> SuggestionHightlights(Guid userId)
        {
            var topSuggestions = await _quizcontext.Suggestions
                .Include(s => s.Language)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .Take(4)
                .Select(s => new SuggestionVm
                {
                    LanguageName = s.Language != null ? s.Language.LanguageName ?? string.Empty : string.Empty,
                    ImprovementTip = s.Suggestions
                })
                .ToListAsync();

            return topSuggestions;
        }

        public async Task<List<SuggestionViewModel>> UserSuggestions(Guid userId)
        {
            var suggestions = await _quizcontext.Suggestions
                .Include(s => s.Language)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .Select(s => new SuggestionViewModel
                {
                    LanguageName = s.Language != null ? s.Language.LanguageName ?? string.Empty : string.Empty,
                    SuggestionText = s.Suggestions,
                    RESourceLink = s.ResourceLink,
                    CreatedOn = s.SavedAt
                })
                .ToListAsync();

            return suggestions;
        }

        public async Task<List<Suggestion>> GetSuggestionsForQuizResultAsync(Guid quizResultId)
        {
            return await _quizcontext.Suggestions
                .Where(s => s.ResultId == quizResultId)
                .ToListAsync();
        }
    }
}

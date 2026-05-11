using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Interface.Service;
using Quiz_Application.Models;
using Quiz_Application.Models.DTO;
using Quiz_Application.Models.DTO.Quiz_Application.Models.DTO;
using System.Text;
using System.Text.Json;

namespace Quiz_Application.Implementation.Service
{
    public class SuggestionService : ISuggestionService
    {
        private readonly IQuizRepository _quizRepository;
        private readonly ISuggestionRepository _suggestionRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiKey;
        private readonly IMemoryCache _memoryCache;
        private readonly QuizContext _dbContext;

        public SuggestionService(
            IQuizRepository quizRepository,
            ISuggestionRepository suggestionRepository,
            ILanguageRepository languageRepository,
            IConfiguration configuration,
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            QuizContext dbContext)
        {
            _quizRepository = quizRepository;
            _suggestionRepository = suggestionRepository;
            _languageRepository = languageRepository;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
            _dbContext = dbContext;

            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        }

        public async Task<List<SuggestionDto>> GetSuggestionsAsync(Guid quizResultId)
        {
            var result = await _quizRepository.GetResultById(quizResultId);
            if (result == null)
            {
                return new List<SuggestionDto>
                {
                    new SuggestionDto { Suggestions = "Quiz result not found.", ResourseLienk = "" }
                };
            }

            var quiz = await _quizRepository.GetQuizById(result.QuizId);
            if (quiz == null)
            {
                return new List<SuggestionDto>
                {
                    new SuggestionDto { Suggestions = "Quiz session not found for this result.", ResourseLienk = "" }
                };
            }

            var language = await _languageRepository.GetLanguageByIdAsync(quiz.LanguageId);
            if (language == null)
            {
                return new List<SuggestionDto>
                {
                    new SuggestionDto { Suggestions = "Technology module not found for this quiz.", ResourseLienk = "" }
                };
            }

            var techName = language.LanguageName ?? "this technology";
            var proficiencyLevel = quiz.Level ?? "Beginner";

            var questions = await _quizRepository.GetQuestionsByQuizId(result.QuizId);
            var wrongAnswers = questions.Select(q => q.QuestionText).ToList();
            var wrongAnswersText = string.Join(" ", wrongAnswers);

            var prompt =
                $"You are an expert, supportive, and motivating learning guide. " +
                $"Your purpose is to help a user learn {techName} and address their specific weaknesses. " +
                $"Based on the following criteria, generate highly relevant learning suggestions. " +
                $"Each suggestion must directly address the user's weaknesses and proficiency level and include a link to a high-quality online resource." +
                $"\n\n**Criteria:**" +
                $"\n1.  **Technology:** {techName}" +
                $"\n2.  **User Level:** {proficiencyLevel}" +
                $"\n3.  **Weaknesses:** {wrongAnswersText}" +
                $"\n\n**Response Format:**" +
                $"\nReturn only valid JSON with this structure: " +
                $"[\n" +
                $"  {{ \"Suggestions\": \"string\", \"ResourseLienk\": \"string\" }}\n" +
                $"]";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.7, maxOutputTokens = 2048 }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return BuildFallbackSuggestions(techName, proficiencyLevel);
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}"
            );
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return BuildFallbackSuggestions(techName, proficiencyLevel);
            }

            var json = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(json);
            var geminiContent = geminiResponse?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;

            if (string.IsNullOrWhiteSpace(geminiContent))
            {
                return BuildFallbackSuggestions(techName, proficiencyLevel);
            }

            var suggestions = ParseSuggestionsJson(geminiContent);
            foreach (var item in suggestions)
            {
                var suggest = new Suggestion()
                {
                    Id = Guid.NewGuid(),
                    UserId = GetCurrentUserId(),
                    LanguageId = language.Id, 
                    CourseId = language.CourseId,
                    ResultId = quizResultId,
                    Suggestions = item.Suggestions,
                    ResourceLink = item.ResourseLienk,
                    SavedAt = DateTime.UtcNow
                };
                await _suggestionRepository.AddAsync(suggest);
            }

            _memoryCache.Set($"Suggestions_{quizResultId}", suggestions, TimeSpan.FromHours(1));
            return suggestions;
        }

        public async Task SaveSuggestionAsync(Guid quizResultId, List<SuggestionDto> suggestionDto)
        {
            var userId = GetCurrentUserId();
            var result = await _quizRepository.GetResultById(quizResultId);
            if (result == null) return;

            var quiz = await _quizRepository.GetQuizById(result.QuizId);
            if (quiz == null) return;

            var language = await _languageRepository.GetLanguageByIdAsync(quiz.LanguageId);
            if (language == null) return;

            foreach (var dto in suggestionDto)
            {
                var suggestion = new Suggestion
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LanguageId = quiz.LanguageId,
                    CourseId = language.CourseId,
                    ResultId = quizResultId,
                    ResourceLink = dto.ResourseLienk,
                    Suggestions = dto.Suggestions,
                    SavedAt = DateTime.UtcNow
                };
                await _suggestionRepository.AddAsync(suggestion);
            }
        }

        public List<SuggestionDto> GetSuggestionsFromCache(Guid quizResultId)
        {
            var cacheKey = $"Suggestions_{quizResultId}";
            if (!_memoryCache.TryGetValue(cacheKey, out List<Suggestion>? suggestions) || suggestions == null)
            {
                suggestions = _dbContext.Suggestions
                    .Where(s => s.ResultId == quizResultId)
                    .ToList();

                _memoryCache.Set(cacheKey, suggestions, TimeSpan.FromHours(1));
            }

            return suggestions.Select(s => new SuggestionDto
            {
                Id = s.Id,
                ResourseLienk = s.ResourceLink,
                Suggestions = s.Suggestions,
                SavedAt = s.SavedAt
            }).ToList();
        }

        private List<SuggestionDto> ParseSuggestionsJson(string geminiContent)
        {
            try
            {
                var cleaned = geminiContent.Replace("```json", "").Replace("```", "").Trim();
                return JsonSerializer.Deserialize<List<SuggestionDto>>(cleaned, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<SuggestionDto>();
            }
            catch
            {
                return new List<SuggestionDto> { new SuggestionDto { Suggestions = "Critical: Neural Stream parsing failure." } };
            }
        }

        private static List<SuggestionDto> BuildFallbackSuggestions(string techName, string level)
        {
            return new List<SuggestionDto>
            {
                new SuggestionDto
                {
                    Suggestions = $"Review the core {techName} concepts for {level} level and retry weak questions from your quiz.",
                    ResourseLienk = "https://learn.microsoft.com/training/"
                },
                new SuggestionDto
                {
                    Suggestions = $"Practice small {techName} examples, then compare your answer with official documentation.",
                    ResourseLienk = "https://developer.mozilla.org/"
                }
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            return Guid.TryParse(userIdString, out Guid userId) ? userId : Guid.Empty;
        }
    }

    public class GeminiResponse { public List<Candidate> candidates { get; set; } = new(); }
    public class Candidate { public Content content { get; set; } = new(); }
    public class Content { public List<Part> parts { get; set; } = new(); }
    public class Part { public string text { get; set; } = string.Empty; }
}

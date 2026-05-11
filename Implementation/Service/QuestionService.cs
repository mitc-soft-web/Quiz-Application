using Quiz_Application.Interface.Repository;
using Quiz_Application.Interface.Service;
using Quiz_Application.Interfaces.Repositories;
using Quiz_Application.Models;
using Quiz_Application.Models.DTO.Question;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;

namespace Quiz_Application.Implementation.Service
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<QuestionService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly string _geminiApiKey;

        public QuestionService(
            IQuestionRepository questionRepository,
            IUnitOfWork unitOfWork,
            ILanguageRepository languageRepository,
            HttpClient httpClient,
            ILogger<QuestionService> logger,
            IConfiguration config,
            IMemoryCache cache)
        {
            _questionRepository = questionRepository;
            _languageRepository = languageRepository;
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _config = config;
            _cache = cache;
            _geminiApiKey = _config["Gemini:ApiKey"] ?? string.Empty;
        }

        public async Task<List<QuestionDTO>> GenerateQuestionsFromApiAsync(Guid languageId, string level, int numberOfQuestions, CancellationToken cancellation)
        {
            var language = await _languageRepository.GetLanguageByIdAsync(languageId);
            if (language == null) throw new Exception("Invalid language");

            numberOfQuestions = Math.Clamp(numberOfQuestions, 20, 150);
            var languageName = language.LanguageName ?? "Technology";

            if (string.IsNullOrWhiteSpace(_geminiApiKey))
            {
                return BuildFallbackQuestions(languageId, languageName, level, numberOfQuestions);
            }

            var questions = new List<QuestionDTO>();
            var batchSize = 20;

            while (questions.Count < numberOfQuestions)
            {
                var remaining = numberOfQuestions - questions.Count;
                var currentBatchSize = Math.Min(batchSize, remaining);
                var prompt = BuildPrompt(languageName, level, currentBatchSize, questions.Count + 1);

                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topP = 0.95,
                        topK = 40,
                        maxOutputTokens = 8192
                    }
                };

                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                    using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}");
                    request.Content = content;

                    var response = await _httpClient.SendAsync(request, cancellation);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellation);
                        _logger.LogWarning("Gemini question generation failed. Status: {StatusCode}. Content: {Content}", response.StatusCode, errorContent);
                        break;
                    }

                    var json = await response.Content.ReadAsStringAsync(cancellation);
                    using var doc = JsonDocument.Parse(json);
                    var geminiContent = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                    var cleanedJson = Regex.Replace(geminiContent ?? "[]", @"^```json|```$", "", RegexOptions.Multiline).Trim();
                    var rawQuestions = JsonSerializer.Deserialize<List<ExternalQuestionModel>>(cleanedJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    var batchQuestions = MapQuestions(rawQuestions, languageId, level, currentBatchSize);

                    if (!batchQuestions.Any())
                    {
                        break;
                    }

                    questions.AddRange(batchQuestions);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Using fallback question fill for language {LanguageId}", languageId);
                    break;
                }
            }

            if (questions.Count < numberOfQuestions)
            {
                var fallback = BuildFallbackQuestions(languageId, languageName, level, numberOfQuestions - questions.Count);
                questions.AddRange(fallback);
            }

            questions = questions.Take(numberOfQuestions).ToList();
            _cache.Set($"QuizQuestions_{languageId}", questions, TimeSpan.FromMinutes(30));
            return questions;
        }

        public async Task<bool> SaveQuestionsAsync(Guid quizId, List<QuestionDTO> questions, CancellationToken cancellation)
        {
            if (questions == null || !questions.Any()) return false;

            var entities = questions.Select(dto => new Question
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectOption = dto.CorrectAnswer,
                QuizId = quizId,
                LanguageId = dto.LanguageId,
                Difficulty = dto.Difficulty
            }).ToList();

            await _questionRepository.AddQuestionsAsync(entities);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<QuestionDTO>> GetQuestionsByQuizIdAsync(Guid quizId, CancellationToken cancellation)
        {
            var list = await _questionRepository.GetQuestionsByQuizIdAsync(quizId);
            return list.Select(q => new QuestionDTO
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectOption,
                QuizId = q.QuizId,
                LanguageId = q.LanguageId,
                Difficulty = q.Difficulty
            });
        }

        public async Task<QuestionDTO?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellation)
        {
            var q = await _questionRepository.GetQuestionByIdAsync(questionId);
            if (q == null) return null;

            return new QuestionDTO
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectOption,
                QuizId = q.QuizId,
                LanguageId = q.LanguageId,
                Difficulty = q.Difficulty
            };
        }

        public async Task<bool> DeleteQuestionAsync(Guid questionId, CancellationToken cancellation)
        {
            var q = await _questionRepository.GetQuestionByIdAsync(questionId);
            if (q == null) return false;

            await _questionRepository.DeleteAsync(q);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<QuestionDTO>> GetQuestionsForUserAsync(int requestedCount, CancellationToken cancellation)
        {
            var questions = await _questionRepository.GetRandomQuestionsAsync(requestedCount);

            return questions.Select(q => new QuestionDTO
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectAnswer = q.CorrectOption,
                QuizId = q.QuizId,
                LanguageId = q.LanguageId,
                Difficulty = q.Difficulty
            });
        }



        private static string BuildPrompt(string languageName, string level, int count, int startNumber)
        {
            return $"Generate exactly {count} multiple-choice quiz questions for the selected technology topic: {languageName}. " +
                   $"The questions must be about {languageName} only, not about a general course category. " +
                   $"Difficulty level: {level}. " +
                   $"Start from question number {startNumber}, but do not include numbering in the JSON. " +
                   $"Each question must have 4 options. " +
                   $"The correctAnswer must be the full text of one of the options. " +
                   $"Return valid JSON only as an array of objects with this shape: " +
                   $"[{{\"question\":\"string\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswer\":\"string\"}}]. " +
                   $"Do not use markdown, explanations, comments, or trailing commas.";
        }

        private static List<QuestionDTO> MapQuestions(List<ExternalQuestionModel>? rawQuestions, Guid languageId, string level, int count)
        {
            if (rawQuestions == null) return new List<QuestionDTO>();

            return rawQuestions
                .Where(q => !string.IsNullOrWhiteSpace(q.Question) && q.Options.Count >= 4)
                .Take(count)
                .Select(q =>
                {
                    var options = q.Options.Take(4).Select(o => o ?? string.Empty).ToList();
                    var correctAnswer = options.FirstOrDefault(o => string.Equals(o, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                        ?? options.First();

                    return new QuestionDTO
                    {
                        Id = Guid.NewGuid(),
                        QuestionText = q.Question,
                        OptionA = options.ElementAtOrDefault(0) ?? string.Empty,
                        OptionB = options.ElementAtOrDefault(1) ?? string.Empty,
                        OptionC = options.ElementAtOrDefault(2) ?? string.Empty,
                        OptionD = options.ElementAtOrDefault(3) ?? string.Empty,
                        CorrectAnswer = correctAnswer,
                        LanguageId = languageId,
                        Difficulty = level
                    };
                })
                .ToList();
        }

        private class ExternalQuestionModel
        {
            public string Question { get; set; } = "";
            public List<string> Options { get; set; } = new();
            public string CorrectAnswer { get; set; } = "";
        }

        private static List<QuestionDTO> BuildFallbackQuestions(Guid languageId, string languageName, string level, int count)
        {
            var templates = new[]
            {
                new { Question = "What is the main purpose of studying {0}?", A = "To understand core concepts and solve related problems", B = "To avoid using documentation", C = "To memorize unrelated facts", D = "To remove all testing", Correct = "To understand core concepts and solve related problems" },
                new { Question = "Which practice is most useful when learning {0}?", A = "Building small practical examples", B = "Skipping fundamentals", C = "Ignoring errors", D = "Avoiding feedback", Correct = "Building small practical examples" },
                new { Question = "At {1} level, what should a learner focus on in {0}?", A = "Concepts, use cases, and correct application", B = "Random guessing", C = "Only UI colors", D = "Deleting previous work", Correct = "Concepts, use cases, and correct application" },
                new { Question = "What helps improve performance in {0}?", A = "Measuring, identifying bottlenecks, and optimizing carefully", B = "Changing code blindly", C = "Removing validation", D = "Ignoring user needs", Correct = "Measuring, identifying bottlenecks, and optimizing carefully" },
                new { Question = "Which approach makes {0} projects easier to maintain?", A = "Clear structure, readable naming, and testing", B = "Duplicating every file", C = "Hiding all errors", D = "Avoiding version control", Correct = "Clear structure, readable naming, and testing" },
                new { Question = "When debugging {0}, what should you do first?", A = "Reproduce the issue and inspect the error message", B = "Delete unrelated files", C = "Ignore logs", D = "Guess until it works", Correct = "Reproduce the issue and inspect the error message" },
                new { Question = "Which habit improves reliability in {0} work?", A = "Testing important behavior before release", B = "Skipping validation", C = "Never reviewing code", D = "Using random configuration", Correct = "Testing important behavior before release" },
                new { Question = "Why is documentation useful in {0}?", A = "It explains expected behavior and decisions", B = "It replaces all testing", C = "It makes errors impossible", D = "It removes the need to understand the topic", Correct = "It explains expected behavior and decisions" },
                new { Question = "What should guide design choices in {0}?", A = "Requirements, constraints, and maintainability", B = "Only visual preference", C = "Copying unrelated projects", D = "Avoiding user needs", Correct = "Requirements, constraints, and maintainability" },
                new { Question = "Which action helps secure a {0} solution?", A = "Validate inputs and apply least privilege", B = "Store secrets in public pages", C = "Disable authentication", D = "Trust every request", Correct = "Validate inputs and apply least privilege" },
                new { Question = "How should a {0} learner handle mistakes?", A = "Analyze the cause and correct the concept", B = "Ignore the mistake", C = "Memorize the wrong answer", D = "Stop practicing", Correct = "Analyze the cause and correct the concept" },
                new { Question = "What makes {0} knowledge practical?", A = "Applying concepts in real scenarios", B = "Only reading definitions", C = "Avoiding exercises", D = "Skipping feedback", Correct = "Applying concepts in real scenarios" },
                new { Question = "Which factor matters when scaling {0} solutions?", A = "Performance, reliability, and resource usage", B = "Longer variable names only", C = "Removing monitoring", D = "Ignoring errors", Correct = "Performance, reliability, and resource usage" },
                new { Question = "What is a good review strategy for {0}?", A = "Check correctness, security, readability, and tests", B = "Approve without reading", C = "Only check colors", D = "Remove all comments", Correct = "Check correctness, security, readability, and tests" }
            };

            return Enumerable.Range(0, count).Select(index =>
            {
                var template = templates[index % templates.Length];
                return new QuestionDTO
                {
                    Id = Guid.NewGuid(),
                    QuestionText = string.Format(template.Question, languageName, level),
                    OptionA = template.A,
                    OptionB = template.B,
                    OptionC = template.C,
                    OptionD = template.D,
                    CorrectAnswer = template.Correct,
                    LanguageId = languageId,
                    Difficulty = level
                };
            }).ToList();
        }
    }
}

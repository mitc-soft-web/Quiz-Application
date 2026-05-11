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

        public async Task<List<QuestionDTO>> GenerateQuestionsFromApiAsync(Guid languageId, string level, int numberOfQuestions, Guid userId, CancellationToken cancellation)
        {
            var language = await _languageRepository.GetLanguageByIdAsync(languageId);
            if (language == null) throw new Exception("Invalid language");

            numberOfQuestions = Math.Clamp(numberOfQuestions, 20, 150);
            var languageName = language.LanguageName ?? "Technology";
            var focusAreas = GetFocusAreas(languageName);
            var previousQuestions = (await _questionRepository.GetPreviousQuestionTextsAsync(userId, languageId))
                .Select(NormalizeQuestion)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(_geminiApiKey))
            {
                return BuildFallbackQuestions(languageId, languageName, level, numberOfQuestions, previousQuestions, focusAreas);
            }

            var questions = new List<QuestionDTO>();
            var batchSize = 20;

            while (questions.Count < numberOfQuestions)
            {
                var remaining = numberOfQuestions - questions.Count;
                var currentBatchSize = Math.Min(batchSize, remaining);
                var prompt = BuildPrompt(languageName, level, currentBatchSize, questions.Count + 1, previousQuestions, focusAreas);

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
                    var currentQuestions = questions.Select(q => NormalizeQuestion(q.QuestionText)).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var blockedQuestions = previousQuestions.Concat(currentQuestions).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var batchQuestions = MapQuestions(rawQuestions, languageId, level, currentBatchSize, blockedQuestions);

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
                var blockedQuestions = previousQuestions
                    .Concat(questions.Select(q => NormalizeQuestion(q.QuestionText)))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                var fallback = BuildFallbackQuestions(languageId, languageName, level, numberOfQuestions - questions.Count, blockedQuestions, focusAreas);
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



        private static string BuildPrompt(string languageName, string level, int count, int startNumber, HashSet<string> previousQuestions, List<string> focusAreas)
        {
            var exclusions = previousQuestions.Any()
                ? "Do not repeat or closely paraphrase these previous questions: " + string.Join(" | ", previousQuestions.Take(40)) + ". "
                : string.Empty;
            var difficultyInstruction = GetDifficultyInstruction(level);
            var focusText = string.Join(", ", focusAreas);

            return $"Generate exactly {count} multiple-choice quiz questions for the selected technology topic: {languageName}. " +
                   $"The questions must be about {languageName} only, not about a general course category. " +
                   $"Cover all these subtopics/focus areas in a mixed way: {focusText}. " +
                   exclusions +
                   $"Mix the question types: definitions, syntax or commands where relevant, debugging, security, practical scenarios, best practices, and real-world problem solving. " +
                   $"Difficulty level: {level}. {difficultyInstruction} " +
                   $"Start from question number {startNumber}, but do not include numbering in the JSON. " +
                   $"Each question must have 4 options. " +
                   $"The correctAnswer must be the full text of one of the options. " +
                   $"Return valid JSON only as an array of objects with this shape: " +
                   $"[{{\"question\":\"string\",\"options\":[\"A\",\"B\",\"C\",\"D\"],\"correctAnswer\":\"string\"}}]. " +
                   $"Do not use markdown, explanations, comments, or trailing commas.";
        }

        private static List<QuestionDTO> MapQuestions(List<ExternalQuestionModel>? rawQuestions, Guid languageId, string level, int count, HashSet<string> blockedQuestions)
        {
            if (rawQuestions == null) return new List<QuestionDTO>();

            return rawQuestions
                .Where(q => !string.IsNullOrWhiteSpace(q.Question) && q.Options.Count >= 4)
                .Where(q => !blockedQuestions.Contains(NormalizeQuestion(q.Question)))
                .Take(count)
                .Select(q =>
                {
                    var options = q.Options.Take(4).Select(o => o ?? string.Empty).ToList();
                    var correctAnswer = options.FirstOrDefault(o => string.Equals(o, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                        ?? options.First();
                    options = ShuffleOptions(options);

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

        private static List<QuestionDTO> BuildFallbackQuestions(Guid languageId, string languageName, string level, int count, HashSet<string> blockedQuestions, List<string> focusAreas)
        {
            var templates = new[]
            {
                new { Question = "In {0}, why is {2} important?", A = "It helps solve real problems correctly", B = "It removes the need to test", C = "It makes every answer automatic", D = "It should be ignored in projects", Correct = "It helps solve real problems correctly" },
                new { Question = "For {1} level {0}, what is the best way to practice {2}?", A = "Build examples and check the result", B = "Guess without testing", C = "Avoid documentation", D = "Skip the basics entirely", Correct = "Build examples and check the result" },
                new { Question = "When debugging {2} in {0}, what should you do first?", A = "Reproduce the issue and inspect the error", B = "Delete unrelated code", C = "Ignore logs", D = "Change random settings", Correct = "Reproduce the issue and inspect the error" },
                new { Question = "Which habit improves reliability when working with {2} in {0}?", A = "Test important behavior before release", B = "Skip validation", C = "Never review code", D = "Use random configuration", Correct = "Test important behavior before release" },
                new { Question = "Which security practice matters for {2} in {0}?", A = "Validate input and use least privilege", B = "Disable authentication", C = "Trust every request", D = "Expose secrets publicly", Correct = "Validate input and use least privilege" },
                new { Question = "What should guide design choices for {2} in {0}?", A = "Requirements, constraints, and maintainability", B = "Only visual preference", C = "Copying unrelated projects", D = "Avoiding user needs", Correct = "Requirements, constraints, and maintainability" }
            };

            var questions = new List<QuestionDTO>();
            var attempt = 0;

            while (questions.Count < count && attempt < count * 4)
            {
                var template = templates[attempt % templates.Length];
                var focusArea = focusAreas[attempt % focusAreas.Count];
                var questionText = string.Format(template.Question, languageName, level, focusArea);
                if (attempt >= templates.Length)
                {
                    questionText = $"{questionText} Scenario {attempt / templates.Length + 1}.";
                }

                var normalized = NormalizeQuestion(questionText);
                attempt++;
                if (blockedQuestions.Contains(normalized))
                {
                    continue;
                }

                blockedQuestions.Add(normalized);
                var options = ShuffleOptions(new List<string> { template.A, template.B, template.C, template.D });

                questions.Add(new QuestionDTO
                {
                    Id = Guid.NewGuid(),
                    QuestionText = questionText,
                    OptionA = options.ElementAtOrDefault(0) ?? string.Empty,
                    OptionB = options.ElementAtOrDefault(1) ?? string.Empty,
                    OptionC = options.ElementAtOrDefault(2) ?? string.Empty,
                    OptionD = options.ElementAtOrDefault(3) ?? string.Empty,
                    CorrectAnswer = template.Correct,
                    LanguageId = languageId,
                    Difficulty = level
                });
            }

            return questions;
        }

        private static List<string> ShuffleOptions(List<string> options)
        {
            return options
                .OrderBy(_ => Random.Shared.Next())
                .ToList();
        }

        private static string GetDifficultyInstruction(string level)
        {
            return level?.Trim().ToLowerInvariant() switch
            {
                "beginner" => "Ask simple foundation questions: meaning, basic syntax, basic usage, simple examples, and common beginner mistakes.",
                "intermediate" => "Ask applied questions: practical use cases, normal project patterns, debugging, data flow, and correct implementation choices.",
                "professional" => "Ask workplace-level questions: architecture decisions, security, maintainability, performance tradeoffs, integration, testing, and debugging production-style issues.",
                "advanced" => "Ask advanced questions: optimization, internals, complex scenarios, scalability, edge cases, and expert-level reasoning.",
                "expert" => "Ask expert questions: deep internals, architecture tradeoffs, failure modes, performance, and scenario-based problem solving.",
                _ => "Match the requested skill level and keep the questions practical."
            };
        }

        private static List<string> GetFocusAreas(string languageName)
        {
            var key = languageName.Trim().ToLowerInvariant();
            var map = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["c# .net"] = new() { "C# syntax", "OOP", "LINQ", "ASP.NET Core MVC", "Web API", "Entity Framework Core", "dependency injection", "async/await", "middleware", "authentication", "validation", "debugging", "unit testing" },
                ["asp.net core mvc"] = new() { "controllers", "actions", "routing", "views", "models", "Razor syntax", "model binding", "validation", "Entity Framework Core", "authentication", "dependency injection", "middleware" },
                ["php"] = new() { "PHP syntax", "forms", "sessions", "arrays", "functions", "OOP", "PDO database access", "Laravel basics", "validation", "authentication", "security", "file handling" },
                ["python"] = new() { "Python syntax", "functions", "lists and dictionaries", "OOP", "exceptions", "file handling", "modules", "virtual environments", "Django basics", "FastAPI basics", "testing", "debugging" },
                ["java"] = new() { "Java syntax", "OOP", "collections", "exceptions", "streams", "Spring Boot", "JPA/Hibernate", "REST APIs", "validation", "testing", "debugging" },
                ["javascript"] = new() { "JavaScript syntax", "DOM", "functions", "arrays", "objects", "promises", "async/await", "events", "fetch API", "Node.js", "debugging", "browser behavior" },
                ["typescript"] = new() { "types", "interfaces", "generics", "classes", "modules", "async logic", "type narrowing", "Node.js", "frontend integration", "debugging" },
                ["c++"] = new() { "syntax", "pointers", "references", "OOP", "STL containers", "memory management", "templates", "exceptions", "performance", "debugging" },
                ["c programming"] = new() { "syntax", "pointers", "arrays", "strings", "structs", "memory allocation", "file I/O", "preprocessor", "debugging", "data structures" },
                ["html5"] = new() { "semantic HTML", "forms", "tables", "media", "accessibility", "SEO basics", "validation", "page structure" },
                ["css3"] = new() { "selectors", "box model", "flexbox", "grid", "responsive design", "variables", "animations", "specificity", "layout debugging" },
                ["react.js"] = new() { "components", "props", "state", "hooks", "events", "forms", "routing", "API calls", "rendering", "performance", "testing" },
                ["angular"] = new() { "components", "templates", "services", "dependency injection", "routing", "forms", "RxJS", "HTTP client", "guards", "testing" },
                ["vue.js"] = new() { "components", "reactivity", "props", "events", "composition API", "routing", "Pinia", "forms", "API calls" },
                ["node.js"] = new() { "runtime", "modules", "npm", "Express", "routing", "middleware", "REST APIs", "authentication", "database access", "error handling" },
                ["sql database"] = new() { "SELECT queries", "joins", "filtering", "aggregation", "indexes", "normalization", "transactions", "stored procedures", "query optimization" },
                ["mysql"] = new() { "tables", "relationships", "SELECT queries", "joins", "indexes", "constraints", "transactions", "stored procedures", "backup basics" },
                ["network security"] = new() { "firewalls", "VPNs", "TLS", "DNS security", "IDS/IPS", "Wireshark", "network attacks", "hardening", "monitoring" },
                ["cybersecurity fundamentals"] = new() { "CIA triad", "authentication", "authorization", "cryptography", "risk assessment", "malware", "incident response", "secure passwords" },
                ["aws cloud services"] = new() { "EC2", "S3", "IAM", "VPC", "Lambda", "RDS", "CloudFront", "security groups", "monitoring", "cost basics" },
                ["microsoft azure"] = new() { "Azure App Service", "Azure Functions", "storage accounts", "Entra ID", "Cosmos DB", "virtual networks", "monitoring", "deployment" },
                ["machine learning"] = new() { "supervised learning", "unsupervised learning", "features", "training", "testing", "metrics", "overfitting", "model evaluation", "scikit-learn" },
                ["artificial intelligence"] = new() { "AI concepts", "machine learning", "neural networks", "NLP", "computer vision", "model evaluation", "prompt engineering", "ethics" }
            };

            if (map.TryGetValue(key, out var focusAreas)) return focusAreas;

            foreach (var pair in map)
            {
                if (key.Contains(pair.Key) || pair.Key.Contains(key))
                {
                    return pair.Value;
                }
            }

            return new List<string>
            {
                "core concepts", "basic syntax or terminology", "practical usage", "debugging",
                "security", "best practices", "testing", "performance", "real-world scenarios"
            };
        }

        private static string NormalizeQuestion(string? question)
        {
            if (string.IsNullOrWhiteSpace(question)) return string.Empty;
            var lower = question.Trim().ToLowerInvariant();
            return Regex.Replace(lower, @"\s+", " ");
        }
    }
}

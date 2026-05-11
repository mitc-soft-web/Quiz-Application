using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Interface.Service;
using Quiz_Application.Models.DTO;
using Quiz_Application.Models.Enum;
using System.Security.Claims;

namespace Quiz_Application.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ILanguageService _languageService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseService courseService,
            ILanguageService languageService,
            ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _languageService = languageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellation)
        {
            try
            {
                var courses = await _courseService.GetAllCoursesAsync(cancellation);
                var tags = await _languageService.GetExternalTagsAsync();

                ViewBag.Tags = tags;
                return View(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load course dashboard");
                TempData["Error"] = "Could not load courses. Please try again.";
                return View(Enumerable.Empty<CourseDTO>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Browse()
        {
            var categories = await _courseService.GetExternalCategoriesAsync();

            return View("Browser", categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(string category, DifficultyLevel difficulty, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                TempData["Error"] = "Please select a category before generating.";
                return RedirectToAction(nameof(Browse));
            }

            try
            {
                var course = await _courseService.GenerateCourseFromExternalApiAsync(category, difficulty, cancellation);
                await _languageService.GenerateLanguagesForCourseAsync(course.Id, category, cancellation);

                _logger.LogInformation("Course generated — {Name} ({Id})", course.CourseName, course.Id);
                TempData["Success"] = $"'{course.CourseName}' created with topics!";
                return RedirectToAction(nameof(Details), new { id = course.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Course generation failed for {Category}", category);
                TempData["Error"] = "AI engine failed. A basic fallback course was created.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellation)
        {
            var course = await _courseService.GetCourseByIdAsync(id, cancellation);

            if (course == null)
            {
                _logger.LogWarning("Data stream access denied: Course {Id} not found.", id);
                TempData["Error"] = "SYSTEM_ERROR: Course ID not recognized. Please re-initialize.";
                return RedirectToAction(nameof(Index));
            }

            await _languageService.GenerateLanguagesForCourseAsync(course.Id, course.CourseName ?? "Technology", cancellation);
            var languages = await _languageService.GetLanguagesByCourseIdAsync(course.Id, cancellation);

            var viewModel = new CourseDetailsViewModel
            {
                Course = course,

                Topics = languages.Select(l => new LanguageDTO
                {
                    Id = l.Id,
                    LanguageName = l.LanguageName ?? string.Empty,
                    CourseId = course.Id
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid courseId, CancellationToken cancellation)
        {
            if (courseId == Guid.Empty) return NotFound();
            var course = await _courseService.GetCourseByIdAsync(courseId, cancellation);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid courseId, CourseDTO courseDto, CancellationToken cancellation)
        {
            if (courseId == Guid.Empty) return NotFound();

            if (string.IsNullOrWhiteSpace(courseDto.CourseName))
            {
                ModelState.AddModelError(nameof(courseDto.CourseName), "Course name is required.");
                return View(courseDto);
            }

            try
            {
                var updated = await _courseService.UpdateCourseAsync(courseId, courseDto, cancellation);
                if (!updated) { TempData["Error"] = "Course not found."; return RedirectToAction(nameof(Index)); }

                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update course {CourseId}", courseId);
                TempData["Error"] = "Update failed. Please try again.";
                return View(courseDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllLanguages(CancellationToken cancellation)
        {
            var languages = await _languageService.GetAllLanguagesAsync(cancellation);
            return View("~/Views/Language/Index.cshtml", languages);
        }

        [HttpGet]
        public async Task<IActionResult> LanguagesByName(string courseName, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(courseName))
                return Json(new List<object>());

            var languages = await _languageService.GetLanguagesByCourseAsync(courseName, cancellation);
            return Json(languages.Select(l => new { id = l.Id, languageName = l.LanguageName }));
        }

        [HttpGet]
        public async Task<IActionResult> LanguageById(Guid languageId, CancellationToken cancellation)
        {
            if (languageId == Guid.Empty) return NotFound();
            var language = await _languageService.GetLanguageByIdAsync(languageId, cancellation);
            if (language == null) return NotFound();
            return RedirectToAction("Index", "Quiz", new { languageId = language.Id, languageName = language.LanguageName });
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    public class CourseDetailsViewModel
    {
        public CourseDTO Course { get; set; } = new();
        public List<LanguageDTO> Topics { get; set; } = new();

    }

    public class LanguageDTO
    {
        public Guid Id { get; set; }
        public string LanguageName { get; set; } = "";
        public Guid CourseId { get; set; }
    }
}

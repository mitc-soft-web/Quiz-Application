using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Interface.Service;
using Quiz_Application.Models.DTO;

namespace Quiz_Application.Controllers
{
    public class ResultController : Controller
    {
        private readonly IResultService _resultService;
        private readonly ILogger<ResultController> _logger;

        public ResultController(
            IResultService resultService,
            ILogger<ResultController> logger)
        {
            _resultService = resultService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid quizId)
        {
            if (quizId == Guid.Empty) return RedirectToAction("Index", "Course");

            try
            {
                var result = await _resultService.GetResultByQuizIdAsync(quizId, default);

                if (result == null)
                {
                    TempData["Error"] = "Data Void: No assessment record found for this session.";
                    return RedirectToAction("Index", "Course");
                }

                return RedirectToAction("Result", "Quiz", new { quizId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Matrix Error: Failed to retrieve result for {QuizId}", quizId);
                return RedirectToAction("Index", "Course");
            }
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return RedirectToAction("Login", "User");

            try
            {
                await _resultService.GetResultsByUserIdAsync(userId, default);
                return RedirectToAction("History", "Quiz");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "History Sync Failure for User {UserId}", userId);
                TempData["Error"] = "Neural History Error: Unable to fetch previous records.";
                return RedirectToAction("Index", "Course");
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdSession = HttpContext.Session.GetString("UserId");
            return Guid.TryParse(userIdSession, out Guid userId) ? userId : Guid.Empty;
        }
    }
}

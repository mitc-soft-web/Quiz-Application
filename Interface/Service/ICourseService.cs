using Quiz_Application.Models.DTO;
using Quiz_Application.Models.Enum;

namespace Quiz_Application.Interface.Service
{
    public interface ICourseService
    {
        Task<CourseDTO> GenerateCourseFromExternalApiAsync(string category, DifficultyLevel difficulty, CancellationToken cancellationToken);
        Task<IEnumerable<CourseDTO>> GetAllCoursesAsync(CancellationToken cancellationToken);
        Task<CourseDTO?> GetCourseByIdAsync(Guid courseId, CancellationToken cancellationToken);
        Task<List<string>> GetExternalCategoriesAsync();
        Task<bool> UpdateCourseAsync(Guid id, CourseDTO courseDTO, CancellationToken cancellation);
    }
}

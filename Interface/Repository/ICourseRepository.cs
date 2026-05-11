using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface ICourseRepository
    {
        Task<Course?> GetCourseByNameAsync(string courseName);
        Task AddCourseAsync(Course course);
        Task SaveAsync();
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(Guid courseId);
    }
}

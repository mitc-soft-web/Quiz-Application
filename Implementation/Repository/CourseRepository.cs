using Microsoft.EntityFrameworkCore;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Models;
using Quiz_Application.Models.BaseEntities; 

namespace Quiz_Application.Implementation.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly QuizContext _quizContext;

        public CourseRepository(QuizContext quizContext)
        {
            _quizContext = quizContext;
        }

        public async Task AddCourseAsync(Course course)
        {
            await _quizContext.Courses.AddAsync(course);
            await SaveAsync(); 
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _quizContext.Courses
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Course?> GetCourseByIdAsync(Guid courseId)
        {
            return await _quizContext.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);
        }



        public async Task<Course?> GetCourseByNameAsync(string courseName)
        {
            return await _quizContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourseName == courseName);
        }

        public async Task SaveAsync()
        {
            await _quizContext.SaveChangesAsync();
        }
    }
}

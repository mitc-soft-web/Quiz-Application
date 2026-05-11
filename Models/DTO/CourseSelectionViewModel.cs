using Quiz_Application.Models.DTO;

namespace Quiz_Application.Models.ViewModels
{
    public class CourseSelectionViewModel
    {
        public List<string> Categories { get; set; } = new();
        public List<CourseDTO> ExistingCourses { get; set; } = new();
    }

    public class LanguageSelectionViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public List<LanguageDTO> Languages { get; set; } = new();
    }
}

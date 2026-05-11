using Quiz_Application.Models.DTO;

namespace Quiz_Application.Interface.Service
{
    public interface ILanguageService
    {
        Task<IEnumerable<LanguageDTO>> GenerateLanguagesFromApiAsync(string courseCategory, CancellationToken cancellationToken);
        Task<IEnumerable<LanguageDTO>> GenerateLanguagesForCourseAsync(Guid courseId, string courseCategory, CancellationToken cancellationToken);
        Task<IEnumerable<LanguageDTO>> GetAllLanguagesAsync(CancellationToken cancellationToken);
        Task<LanguageDTO?> GetLanguageByIdAsync(Guid languageId, CancellationToken cancellationToken);
        Task<LanguageDTO?> GetLanguageByNameAsync(string languageName, CancellationToken cancellationToken);
        Task<List<string>> GetExternalTagsAsync();
        Task<IEnumerable<LanguageDTO>> GetLanguagesByCourseAsync(string courseName, CancellationToken cancellationToken);
        Task<IEnumerable<LanguageDTO>> GetLanguagesByCourseIdAsync(Guid courseId, CancellationToken cancellationToken);
    }
}

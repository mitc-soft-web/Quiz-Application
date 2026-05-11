using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quiz_Application.Models;

namespace Quiz_Application.Interface.Repository
{
    public interface ILanguageRepository
    {
        Task<IEnumerable<Language>> GetAllLanguagesAsync();
        Task<IEnumerable<Language>> GetLanguagesByCourseNameAsync(string courseName);
        Task<IEnumerable<Language>> GetLanguagesByCourseIdAsync(Guid courseId);
        Task<Language?> GetLanguageByIdAsync(Guid languageId);
        Task AddLanguageAsync(Language language);
        Task SaveAsync();
        Task<Language?> GetLanguageByNameAsync(string languageName);
    }
}

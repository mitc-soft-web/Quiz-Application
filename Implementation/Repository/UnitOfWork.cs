using Microsoft.EntityFrameworkCore.Storage;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Interfaces.Repositories;

namespace Quiz_Application.Implementation.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly QuizContext _quizContext;

        public UnitOfWork(QuizContext quizContext)
        {
            _quizContext = quizContext ?? throw new ArgumentNullException(nameof(quizContext));
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _quizContext.Database.BeginTransactionAsync();
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _quizContext.SaveChangesAsync(cancellationToken);
        }
    }
}

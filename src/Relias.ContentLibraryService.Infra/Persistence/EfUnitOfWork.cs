using Relias.ContentLibraryService.Common.Interfaces;

namespace Relias.ContentLibraryService.Infra.Persistence;

public class EfUnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

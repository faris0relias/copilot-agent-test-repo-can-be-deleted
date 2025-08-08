using Microsoft.AspNetCore.Mvc.Filters;

namespace Relias.ContentLibraryService.Infra.Persistence
{
    public class DBSaveChangesFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _dbContext;

        public DBSaveChangesFilter(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var result = await next();
            if (result.Exception == null || result.ExceptionHandled)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
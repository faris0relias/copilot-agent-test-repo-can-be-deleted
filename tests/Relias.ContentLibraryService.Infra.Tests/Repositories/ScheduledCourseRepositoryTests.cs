using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Infra.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Relias.ContentLibraryService.Infra.Tests.Repositories;

public class ScheduledCourseRepositoryTests
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ScheduledCourseRepository _repository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;

    public ScheduledCourseRepositoryTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
        });

        var serviceProvider = services.BuildServiceProvider();
        _dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        _repository = new ScheduledCourseRepository(_dbContext);
    }

    [Fact]
    public async Task CreateAsync_AddsScheduledCourseToDbContext()
    {
        var scheduledCourse = new ScheduledCourse
        {
            CourseId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddHours(1),
            StatusId = 3
        };

        await _repository.CreateAsync(scheduledCourse, _cancellationToken);
        var result = await _dbContext.ScheduledCourses.FirstOrDefaultAsync();

        Assert.NotNull(result);
        Assert.Equal(scheduledCourse.CourseId, result.CourseId);
        Assert.Equal(scheduledCourse.StatusId, result.StatusId);
        Assert.Equal(scheduledCourse.ScheduledDate, result.ScheduledDate);
    }

    [Fact]
    public async Task DeleteByCourseIdAndStatusIdAsync_RemovesMatchingRecord()
    {
        var courseId = Guid.NewGuid();
        var statusId = 3;

        var scheduledCourse = new ScheduledCourse
        {
            CourseId = courseId,
            StatusId = statusId,
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        };

        _dbContext.ScheduledCourses.Add(scheduledCourse);
        await _dbContext.SaveChangesAsync();

        await _repository.DeleteByCourseIdAndStatusIdAsync(courseId, statusId, _cancellationToken);

        var result = await _dbContext.ScheduledCourses
            .FirstOrDefaultAsync(sc => sc.CourseId == courseId && sc.StatusId == statusId);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteByCourseIdAndStatusIdAsync_DoesNothingIfNoMatch()
    {
        var courseId = Guid.NewGuid();
        var statusId = 3;

        await _repository.DeleteByCourseIdAndStatusIdAsync(courseId, statusId, _cancellationToken);

        Assert.Empty(_dbContext.ScheduledCourses);
    }
}

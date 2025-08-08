using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.Api.Filters;

public class ReliasCourseFilterAttribute : TypeFilterAttribute<ReliasCourseFilter> { }

public class ReliasCourseFilter : IAsyncActionFilter
{
    private readonly ICourseService _courseService;
    private readonly ILogger<ReliasCourseFilter> _logger;

    public ReliasCourseFilter(ICourseService courseService,
        ILogger<ReliasCourseFilter> logger)
    {
        _courseService = courseService;
        _logger = logger;

    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        Guid courseId = Guid.Empty;
        const string invalidCourseIdMessage = "CourseId not found or invalid in route.";
        const string courseNotFoundMessage = "Course not found.";
        const string cannotModifyReliasCourseMessage = "Relias-owned courses cannot be modified or deleted.";

        bool hasCourseId = context.ActionArguments.TryGetValue("courseId", out var courseIdValue) &&
                           Guid.TryParse(courseIdValue?.ToString(), out courseId);

        if (!hasCourseId)
        {
            _logger.LogWarning(invalidCourseIdMessage);
            context.Result = new NotFoundObjectResult(invalidCourseIdMessage);
            return;
        }

        CourseDto? course = await _courseService.GetByIdAsync(courseId, context.HttpContext.RequestAborted);

        if (course == null)
        {
            _logger.LogWarning("Course with ID {CourseId} not found.", courseId);
            context.Result = new NotFoundObjectResult(courseNotFoundMessage);
            return;
        }

        bool isReliasOwnedCourse = course!.IsRelias;

        if (isReliasOwnedCourse)
        {
            _logger.LogWarning("Relias owned course {CourseId} cannot be modified or deleted.", courseId);
            context.Result = new BadRequestObjectResult(cannotModifyReliasCourseMessage);
            return;
        }

        await next();
    }
}

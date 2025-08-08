using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Relias.ContentLibraryService.Api.Authorization;
using Relias.ContentLibraryService.Api.Filters;
using Relias.ContentLibraryService.Api.Versioning;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Commands.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Dtos.LearningContent;
using Relias.ContentLibraryService.App.Features.Course.Queries;
using Relias.ContentLibraryService.App.Features.Course.Queries.Learner;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Relias.ContentLibraryService.Api.Controllers.V1;

[ExcludeFromCodeCoverage]
[ApiV1]
public class CoursesController(IMediator mediator, IMapper mapper) : ApiControllerBase(mediator)
{
    /// <summary>
    /// Gets all courses.
    /// </summary>
    /// <returns>List of courses.</returns>
    [HttpGet("list")]
    [Authorize(Policy = AuthorizationPolicyNames.OrganizationAccess)]
    [ProducesResponseType(typeof(List<CourseDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ListCourses([FromQuery] int organizationId, CancellationToken cancellationToken)
    {
        var query = new GetCoursesQuery.Contract
        {
            OrganizationId = organizationId
        };
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a course by contentId.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Course details.</returns>
    [HttpGet("{courseId}")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(CourseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetCourseById(Guid courseId, CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery.Contract
        {
            CourseId = courseId
        };

        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Creates a new course.
    /// </summary>
    /// <returns>Newly created course.</returns>
    [HttpPost]
    [Authorize(AuthorizationPolicyNames.OrganizationAccess)]
    [ProducesResponseType(typeof(CourseDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto newCourse, CancellationToken cancellationToken)
    {

        CreateCourseCommand.Contract command = new()
        {
            NewCourse = newCourse
        };
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"{Request.Path}/{result.CourseId}", result);
    }

    /// <summary>
    /// Updates a course by courseId.
    /// </summary>
    /// <returns>Updated course.</returns>
    [HttpPut("{courseId}")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ReliasCourseFilter]
    [ProducesResponseType(typeof(CourseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateCourseById([FromRoute] Guid courseId, [FromBody] UpdateCourseDto courseDto, CancellationToken cancellationToken)
    {
        var command = new UpdateCourseCommand.Contract
        {
            UpdatedCourse = new UpdateCourseDto 
            {
                Title = courseDto.Title,
                ContentId = courseDto.ContentId, 
                ContentCode = courseDto.ContentCode, 
                BriefDescription = courseDto.BriefDescription, 
                LanguageIds=courseDto.LanguageIds, 
                Description = courseDto.Description, 
                OrganizationId = courseDto.OrganizationId 
            },
            CourseId = courseId
        };

        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Archives a course by Id.
    /// </summary>
    /// <returns>No content response.</returns>
    [HttpPost("{courseId}/archive")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> ArchiveCourseById([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        // TODO: Not implemented

        var command = new ArchiveCourseCommand.Contract
        {
            CourseId = courseId
        };
        await Mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a course with the specified course ID.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A 204 No Content response if the course was successfully deleted.
    /// A 404 Not Found response if the course does not exist.
    /// </returns>
    [HttpDelete("{courseId}")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ReliasCourseFilter]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> DeleteCourse([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var command = new DeleteCourseCommand.Contract(courseId);

        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets learning content for a specific course.
    /// </summary>
    /// <param name="courseId">The unique identifier of the course.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Learning content associated with the course.</returns>
    [HttpGet("{courseId}/learning-content")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(LearningContentDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetLearningContentByCourseId([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var query = new GetLearningContentByCourseIdQuery.Contract
        {
            CourseId = courseId
        };

        var result = await Mediator.Send(query, cancellationToken);

        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Handles adding, updating, reordering, and deleting sections for a given course
    /// Can reorder learning objects within sections
    /// </summary>
    /// <returns>Updated learning content.</returns>
    [HttpPatch("{courseId}/learning-content/section")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(LearningContentDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateLearningContent([FromRoute] Guid courseId, [FromBody] JsonPatchDocument updates, CancellationToken cancellationToken)
    {
        var command = new UpdateLearningContentCommand.Contract
        {
            CourseId = courseId,
            Updates = updates
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds a lesson to a learning content section by courseId and sectionId
    /// </summary>
    /// <returns>Updated learning content.</returns>
    [HttpPost("{courseId}/learning-content/section/{sectionId}/lesson")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(LessonDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> AddLessonToSection([FromRoute] Guid courseId, [FromRoute] Guid sectionId, [FromBody] CreateLessonDto lesson, CancellationToken cancellationToken)
    {
        var command = new CreateLessonCommand.Contract
        {
            CourseId = courseId,
            SectionId = sectionId,
            LessonDto = lesson
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update a lesson by courseId and sectionId
    /// </summary>
    /// <returns>Updated lesson.</returns>
    [HttpPut("{courseId}/learning-content/section/{sectionId}/lesson/{learningObjectId}")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(LessonDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> UpdateLesson([FromRoute] Guid courseId, [FromRoute] Guid sectionId, [FromRoute] Guid learningObjectId, [FromBody] UpdateLessonDto lesson, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateLessonCommand.Contract
            {
                CourseId = courseId,
                SectionId = sectionId,
                LearningObjectId = learningObjectId,
                LessonDto = lesson
            };

            var result = await Mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Log exception if logger is available
            // logger.LogError(ex, "Error updating lesson");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // logger.LogError(ex, "Unexpected error updating lesson");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred while updating the lesson." });
        }
    }

    /// <summary>
    /// Deletes a lesson from a learning content section by courseId, sectionId, and learningObjectId.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="sectionId">The ID of the section.</param>
    /// <param name="learningObjectId">The ID of the learning object (lesson).</param>
    /// <param name="organizationId">The ID of the organization (from query).</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A 204 No Content response if the lesson was successfully deleted.
    /// A 404 Not Found response if the lesson, section, or course does not exist.
    /// </returns>
    [HttpDelete("{courseId}/learning-content/section/{sectionId}/lesson/{learningObjectId}")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<IActionResult> DeleteLesson([FromRoute] Guid courseId, [FromRoute] Guid sectionId, [FromRoute] Guid learningObjectId, [FromQuery] int organizationId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteLessonCommand.Contract(courseId, sectionId, learningObjectId, organizationId);
            await Mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // logger.LogError(ex, "Error deleting lesson");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // logger.LogError(ex, "Unexpected error deleting lesson");
            return StatusCode((int)HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred while deleting the lesson." });
        }
    }

    /// <summary>
    /// Uploads a lesson file to blob storage.
    /// </summary>
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB limit
    [HttpPost("{courseId}/learning-content/lesson/{learningObjectId}/upload-file")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLessonFile(
        [FromRoute] Guid courseId,
        [FromRoute] Guid learningObjectId,
        [FromQuery] int organizationId,
        IFormFile chunk,
        [FromForm] string uploadId,
        [FromForm] int chunkIndex,
        [FromForm] int totalChunks,
        [FromForm] string fileName,
        CancellationToken cancellationToken)
    {
        var command = new UploadLessonFileCommand.Contract
        {
            CourseId = courseId,
            LearningObjectId = learningObjectId,
            OrganizationId = organizationId,
            Chunk = chunk,
            UploadId = uploadId,
            ChunkIndex = chunkIndex,
            TotalChunks = totalChunks,
            FileName = fileName
        };

        await Mediator.Send(command, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Gets a final exam by course id.
    /// </summary>
    /// <param name="courseId">The course ID of the final exam.</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>A <see cref="FinalExamDto"/> if it exists, otherwise <c>null</c>.</returns>
    [HttpGet("{courseId}/final-exam")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(FinalExamDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetFinalExamByCourseId([FromRoute] Guid courseId, [FromQuery] string? culture,
        CancellationToken cancellationToken)
    {
        var acceptLanguage = Request.GetTypedHeaders().AcceptLanguage?.FirstOrDefault()?.Value.Value;
        var lang = culture ?? acceptLanguage?.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()
          ?? "en";
        var query = new GetFinalExamByCourseIdQuery.Contract { CourseId = courseId, lang = lang };
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets a final exam question with option details for learner by questionIds and courseId.
    /// </summary>
    /// <param name="courseId">The course ID of the final exam.</param>
    /// <param name="learnerFinalExamQueryDto">The DTO with list of QuestionId and IsCompleted of the final exam.</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>A <see cref="Object"/> if it exists, otherwise <c>null</c>.</returns>
    [HttpPost("{courseId}/learner/final-exam/question-details")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(Object), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetLearnerFinalExamByCourseId([FromRoute] Guid courseId, [FromBody] GetLearnerFinalExamQueryDto learnerFinalExamQueryDto,[FromQuery] string? culture,
        CancellationToken cancellationToken)
    {
        var acceptLanguage = Request.GetTypedHeaders().AcceptLanguage?.FirstOrDefault()?.Value.Value;
        var lang = culture ?? acceptLanguage?.Split(',').FirstOrDefault()?.Split('-').FirstOrDefault()
          ?? "en";

        var query = new GetLearnerFinalExamQuery.Contract { CourseId = courseId, lang= lang,QuestionIds = learnerFinalExamQueryDto.QuestionIds, IsCompleted = learnerFinalExamQueryDto.IsCompleted };
        var result = await Mediator.Send(query, cancellationToken);
        if (result != null)
            return Ok(result);
        else
            return NotFound();
    }

    /// <summary>
    /// Gets a final exam questionIds with correct optionIds for learner by courseId.
    /// </summary>
    /// <param name="courseId">The course ID of the final exam.</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>A <see cref="FinalExamSettingWithAnswerDto"/> if it exists, otherwise <c>null</c>.</returns>
    [HttpGet("{courseId}/learner/final-exam/question-answer-ids")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(FinalExamSettingWithAnswerDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetLearnerFinalExamSettingWithAnswerIdByCourseId([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var query = new GetLearnerFinalExamSettingWithAnswerIdByCourseIdQuery.Contract { CourseId = courseId };
        var result = await Mediator.Send(query, cancellationToken);
        if (result != null)
            return Ok(result);
        else
            return NotFound();
    }

    /// <summary>
    /// Gets a final exam settings for learner.
    /// </summary>
    /// <param name="courseId">The course ID of the final exam.</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>A <see cref="FinalExamSettingDto"/> if it exists, otherwise <c>null</c>.</returns>
    [HttpGet("{courseId}/learner/final-exam/settings")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(Object), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetLearnerFinalExamSettingByCourseId([FromRoute] Guid courseId,
        CancellationToken cancellationToken)
    {
        var query = new GetLearnerFinalExamSettingQuery.Contract { CourseId = courseId };
        var result = await Mediator.Send(query, cancellationToken);
        if (result != null)
            return Ok(result);
        else
            return NotFound();
    }


    /// <summary>
    /// Creates a new final exam.
    /// </summary>
    /// <param name="courseId">The course ID of the final exam.</param>
    /// <param name="organizationId">The identifier of the organization making the update request (from query).</param>
    /// <param name="cancellationToken">The cancellation token to be used.</param>
    /// <returns>A <see cref="FinalExamDto"/> if created successfully, otherwise <c>null</c>.</returns>
    [HttpPost("{courseId}/final-exam")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(FinalExamDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateFinalExam([FromRoute] Guid courseId, [FromQuery] int organizationId, CancellationToken cancellationToken)
    {
        CreateFinalExamCommand.Contract command = new()
        {
            CourseId = courseId,
            OrganizationId = organizationId
        };
        var result = await Mediator.Send(command, cancellationToken);

        return Created($"{Request.Path}/{result}", result);
    }

    /// <summary>
    /// Updates the properties of a final exam for a specific course.
    /// </summary>
    /// <param name="courseId">The unique identifier of the course associated with the final exam.</param>
    /// <param name="organizationId">The identifier of the organization making the update request (from query).</param>
    /// <param name="dataToUpdate">The Dictionary containing the updated properties of the final exam.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation if needed.</param>
    /// <returns>
    /// A <see cref="FinalExamDto"/> representing the updated final exam if the operation is successful, 
    /// otherwise a <c>BadRequest</c> response indicating invalid input or an invalid course state.
    /// </returns>
    [HttpPatch("{courseId}/final-exam")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType(typeof(FinalExamDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> UpdateFinalExam([FromRoute] Guid courseId, [FromQuery] int organizationId, [FromBody] Dictionary<string, dynamic> dataToUpdate, CancellationToken cancellationToken)
    {
        var mappedData = mapper.Map<FinalExamDto>(dataToUpdate);
        var command = new UpdateFinalExamCommand.Contract
        {
            CourseId = courseId,
            UpdatedValues = dataToUpdate,
            UpdatedFinalExam = mappedData,
            OrganizationId = organizationId
        };
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete final exam
    /// </summary>
    /// <param name="courseId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A 204 No Content response if the final exam was successfully deleted.
    /// A 404 Not Found response if the final exam does not exist.
    /// </returns>
    [HttpDelete("{courseId}/final-exam")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    public async Task<ActionResult> DeleteFinalExam([FromRoute] Guid courseId, CancellationToken cancellationToken)
    {
        var command = new DeleteFinalExamCommand.Contract(courseId);

        await Mediator.Send(command, cancellationToken);

        return  Ok();
    }
    /// <summary>
    /// Add or Update FinalExamQuestion for a course by courseId and examId.
    /// </summary>
    /// <param name="courseId"></param>
    /// <param name="examId"></param>
    /// <param name="questionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A 204 No Content response if the final exam was successfully deleted.
    /// A 404 Not Found response if the final exam does not exist.
    /// </returns>
    /// <returns>Updated Final Exam.</returns>
    [HttpPatch("{courseId}/final-exam/{examId}/question/{questionId?}")]
    [ProducesResponseType(typeof(FinalExamDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    public async Task<IActionResult> UpdateFinalExamQuestion([FromRoute] Guid courseId, [FromRoute] Guid examId, [FromRoute] Guid questionId, [FromBody] FinalExamQuestionPatchDto updates, CancellationToken cancellationToken)
    {
        var command = new UpdateFinalExamQuestionCommand.Contract
        {
            CourseId = courseId,
            ExamId = examId,
            QuestionId = questionId,
            Updates = updates
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Publishes a course immediately or schedules it for publishing.
    /// </summary>
    /// <param name="courseId">The ID of the course.</param>
    /// <param name="publishCourse">Details for publishing or scheduling the course.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>204 No Content on success.</returns>
    [HttpPost("{courseId}/publish")]
    [Authorize(Policy = AuthorizationPolicyNames.CourseOrganizationAccess)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> PublishCourse([FromRoute] Guid courseId, [FromBody] PublishCourseDto publishCourse, CancellationToken cancellationToken)
    {
        var command = new PublishCourseCommand.Contract
        {
            CourseId = courseId,
            PublishCourse = publishCourse
        };

        await Mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

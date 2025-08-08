using AutoMapper;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam.Learner;
using Relias.ContentLibraryService.App.Features.Course.Enums;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Enums;
using Relias.ContentLibraryService.Common.Exceptions;
using Relias.ContentLibraryService.Common.Services;
using Relias.ContentLibraryService.Domain.Course;
using Relias.ContentLibraryService.Domain.Course.Enums;
using Relias.ContentLibraryService.Domain.Course.FinalExam;

namespace Relias.ContentLibraryService.App.Services;

public class FinalExamService(IFinalExamRepository finalExamRepository,
                              ILogger<FinalExamService> logger,
                              ICourseRepository courseRepository,
                              ICurrentUserService currentUserService,
                              IMapper mapper, ILanguageContextService languageContextService) : IFinalExamService
{
    public async Task<FinalExamDto?> GetFinalExamByCourseIdAsync(Guid courseId, string lang, CancellationToken cancellationToken)
    {
        languageContextService.SetCurrentLanguage(lang);
        var finalExam = await finalExamRepository.GetFinalExamByCourseIdAsync(courseId, cancellationToken);

        if (finalExam is null)
        {
            logger.LogInformation("Unable to find final exam with course id {CourseId}", courseId);
            return null;
        }

        return mapper.Map<FinalExamDto>(finalExam);

    }

    public async Task<FinalExamSettingDto?> GetFinalExamSettingsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var finalExam = await finalExamRepository.GetFinalExamSettingsAsync(courseId, cancellationToken);

        if (finalExam is null)
        {
            logger.LogInformation("Unable to find learner final exam setting with course id {CourseId}", courseId);
            return null;
        }

        return mapper.Map<FinalExamSettingDto>(finalExam);

    }

    public async Task<FinalExamSettingWithAnswerDto?> GetLearnerFinalExamQuestionAnswerIdsAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var finalExam = await finalExamRepository.GetLearnerFinalExamQuestionAnswerIdsAsync(courseId, cancellationToken);

        if (finalExam?.FinalExamQuestions == null || finalExam.FinalExamQuestions.Count == 0)
        {
            logger.LogInformation("Unable to find final exam with course id {CourseId}", courseId);
            return null;
        }

        return mapper.Map<FinalExamSettingWithAnswerDto>(finalExam);
    }


    public async Task<Object?> GetQuestionsByIdsAsync(
    Guid courseId, List<Guid> questionIds, bool isCompleted, string lang, CancellationToken cancellationToken)
    {
        languageContextService.SetCurrentLanguage(lang);
        var finalExam = await finalExamRepository.GetLearnerFinalExamByCourseIdAsync(
            courseId, questionIds, isCompleted, cancellationToken);

        if (finalExam is null)
        {
            logger.LogInformation("Unable to find final exam for learner with course id {CourseId}", courseId);
            return null;
        }

        return isCompleted
            ? mapper.Map<FinalExamDto>(finalExam)
            : mapper.Map<FinalExamLearnerDto>(finalExam);
    }

    
    public async Task<FinalExamDto> CreateFinalExamAsync(Guid courseId, int organizationId, CancellationToken cancellationToken)
    {
        var course = await courseRepository.GetCourseByCourseIdAsync(courseId, organizationId, cancellationToken);
        if (course is null) throw new BadRequestException($"You can't add final exam. Course with id {courseId} does not exist");

        var duplicate = await finalExamRepository.GetFinalExamByCourseIdAsync(courseId, cancellationToken);
        if (duplicate is not null) throw new BadRequestException($"Final exam with id {courseId} already exists!");

        FinalExam newExam = await finalExamRepository.CreateFinalExamAsync(courseId, currentUserService.UserId, cancellationToken);

        logger.LogInformation("Successfully added the Final Exam for course {CourseId}", courseId);

        await courseRepository.TriggerCourseContentUpdateAsync(courseId, cancellationToken);

        return  mapper.Map<FinalExamDto>(newExam);
    }

    public async Task<FinalExamDto> UpdateFinalExamAsync(Guid courseId, int organizationId, Dictionary<string, dynamic> finalExam, CancellationToken cancellationToken)
    {

        var course = await courseRepository.GetCourseByCourseIdAsync(courseId, organizationId, cancellationToken);
        if (course is null) throw new BadRequestException($"Unable to find course with course id {courseId}");

        if (course.StatusId != (byte)StatusIdEnums.Draft) throw new BadRequestException($"Course with id {courseId} not in draft status!");

        FinalExam updatedExam = await finalExamRepository.UpdateFinalExamAsync(courseId, currentUserService.UserId, finalExam, cancellationToken);
        logger.LogInformation("Successfully updated the Final Exam for course {CourseId}", courseId);

        await courseRepository.TriggerCourseContentUpdateAsync(courseId, cancellationToken);

        return mapper.Map<FinalExamDto>(updatedExam);
    }

    public async Task DeleteFinalExamAsync(Guid courseId, CancellationToken cancellationToken)
    {
        var existing = await finalExamRepository.GetFinalExamByCourseIdAsync(courseId, cancellationToken);
        if (existing is null)
        {
            logger.LogDebug("Final exam with ID {CourseId} not found", courseId);
            throw new NotFoundException("Final Exam not found.");
        }

        await finalExamRepository.DeleteFinalExamAsync(existing, cancellationToken);
        logger.LogInformation("Successfully deleted the Final Exam for course {courseId}", courseId);
    }

    public async Task<FinalExamDto> UpdateFinalExamQuestionAsync(Guid courseId, Guid examId, Guid questionId, FinalExamQuestionPatchDto updates, CancellationToken cancellationToken)
    {

        var existing = await finalExamRepository.GetFinalExamByCourseIdAsync(courseId, cancellationToken);
        if (existing is null) throw new BadRequestException($"Unable to find final exam with course id  {courseId}");

        var QuestionId = questionId == Guid.Empty ? Guid.NewGuid() : questionId;

        if (updates.QuestionUpdate != null)
        {
            updates.QuestionUpdate.QuestionId = QuestionId;
            UpdateQuestion(existing, updates.QuestionUpdate);
        }

        if (updates.QuestionOptionsUpdate?.Count > 0)
        {
            var question = existing.FinalExamQuestions?.FirstOrDefault(q => q.QuestionId == QuestionId)
                ?? throw new BadRequestException($"Question not found in Final Exam. QuestionId: {QuestionId}");

            foreach (var optionUpdate in updates.QuestionOptionsUpdate)
            {
                UpdateQuestionOption(question.QuestionOptions!, optionUpdate);
            }
            ValidateCorrectOptions(question);
        }

        var updated = await finalExamRepository.UpdateFinalExamQuestionAsync(existing, currentUserService.UserId, cancellationToken);
        await courseRepository.TriggerCourseContentUpdateAsync(courseId, cancellationToken);

        return mapper.Map<FinalExamDto>(updated);
    }

    private void UpdateQuestion(FinalExam existing, FinalExamUpdateQuestionDto update)
    {
        switch (update.Action)
        {
            case PatchAction.Remove:
                var QuestionToRemove = existing.FinalExamQuestions?.FirstOrDefault(s => s.QuestionId == update.QuestionId)
                    ?? throw new BadRequestException($"Question not found in Final Exam. QuestionId: {update.QuestionId}");
                existing.FinalExamQuestions.Remove(QuestionToRemove);
                break;

            case PatchAction.Add:


                if (update.QuestionText == null || string.IsNullOrWhiteSpace(update.QuestionText.En))
                    throw new BadRequestException("Question Text is required.");

                if (existing.FinalExamQuestions?.Count >= 200)
                    throw new BadRequestException("Cannot add more than 200 questions.");

                var newQuestion = new FinalExamQuestion
                {
                    QuestionId = update.QuestionId,
                    QuestionType = update.QuestionType,
                    QuestionText = mapper.Map<LocalizedString>(update.QuestionText),
                    QuestionOptions = []
                };
                existing.FinalExamQuestions?.Add(newQuestion);
                break;

            case PatchAction.Replace:
                if (update.QuestionText == null || string.IsNullOrWhiteSpace(update.QuestionText.En))
                    throw new BadRequestException("Question Text is required.");

                var existingQuestion = existing.FinalExamQuestions?.FirstOrDefault(s => s.QuestionId == update.QuestionId)
                    ?? throw new BadRequestException($"Question not found in Final Exam. QuestionId: {update.QuestionId}");

                mapper.Map(update, existingQuestion);
                break;
        }
    }

    private void UpdateQuestionOption(List<FinalExamQuestionOptions> options, FinalExamQuestionUpdateOptionDto update)
    {
        switch (update.Action)
        {
            case PatchAction.Add:
                
                if (options.Count >= 50)
                    throw new BadRequestException("Question must contain at least 2 options and a maximum of 50.");

                var newOption = new FinalExamQuestionOptions
                {
                    OptionId = Guid.NewGuid(),
                    OptionText = mapper.Map<LocalizedString>(update.OptionText),
                    ResponseFeedback = mapper.Map<LocalizedString>(update.ResponseFeedback),
                    IsCorrect = update.IsCorrect
                };
                options.Add(newOption);
                break;

            case PatchAction.Replace:
                var existingOption = options.FirstOrDefault(o => o.OptionId == update.OptionId)
                    ?? throw new BadRequestException($"Option not found in Question. OptionId: {update.OptionId}");
                mapper.Map(update, existingOption);
                break;

            case PatchAction.Remove:
                if (options.Count <= 2)
                    throw new BadRequestException("Cannot remove option. A question must have at least two options.");

                var optionToRemove = options.FirstOrDefault(o => o.OptionId == update.OptionId)
                    ?? throw new BadRequestException($"Option not found in question. OptionId: {update.OptionId}");

                options.Remove(optionToRemove);
                break;
        }
    }
    private static void ValidateCorrectOptions(FinalExamQuestion question)
    {
        var correctCount = question.QuestionOptions?.Count(o => o.IsCorrect== true) ?? 0;

        switch (question.QuestionType)
        {
            case QuestionType.SingleSelect when correctCount != 1:
                throw new BadRequestException("Single select question must have exactly one correct option.");

            case QuestionType.MultiSelect when correctCount < 1:
                throw new BadRequestException("Multi select question must have at least one correct option.");
        }
    }



}

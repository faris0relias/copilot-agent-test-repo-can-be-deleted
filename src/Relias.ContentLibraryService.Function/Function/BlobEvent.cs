using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Constants;
using Relias.ContentLibraryService.Function.Models;
using System.Text.Json;

namespace Relias.ContentLibraryService.Function.Function
{
    public class BlobEvent(
        ILogger<BlobEvent> logger, 
        IMainBlobStorageRepository repository
        )
    {
        private const string AntimalwareScanEventType = "Microsoft.Security.MalwareScanningResult";
        private const string MaliciousVerdict = "Malicious";
        private const string FailedVerdict = "Failed";
        private const string CleanVerdict = "No threats found";

        /// <summary>
        /// Handles EventGrid events for blob malware scanning results.
        /// </summary>
        /// <param name="eventGridEvent">The EventGrid event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        [Function("BlobEventTrigger")]
        public async Task BlobResultEvent([EventGridTrigger] EventGridEvent eventGridEvent, CancellationToken cancellationToken)
        {
            if (eventGridEvent.EventType != AntimalwareScanEventType)
            {
                logger.LogInformation("Event type is not an {0} event, event type:{1}", AntimalwareScanEventType, eventGridEvent.EventType);
                return;
            }

            var eventData = JsonSerializer.Deserialize<ScanResultEventData>(eventGridEvent.Data.ToString());
            if (eventData == null || string.IsNullOrWhiteSpace(eventData.scanResultType) || string.IsNullOrWhiteSpace(eventData.blobUri))
            {
                logger.LogError("Event data contains empty or invalid 'scanResultType' or 'blobUri' values");
                throw new ArgumentException("Event data contains empty or invalid 'scanResultType' or 'blobUri' values");
            }
            var verdict = eventData.scanResultType;
            var blobUriString = eventData.blobUri;

            if (verdict != MaliciousVerdict && verdict != FailedVerdict && verdict != CleanVerdict)
            {
                logger.LogError("Event data contains unexpected 'scanResultType' value: {Verdict}", verdict);
                throw new ArgumentException($"Event data contains unexpected 'scanResultType' value: {verdict}");
            }

            var blobUri = new Uri(blobUriString);
            var blobUriBuilder = new BlobUriBuilder(blobUri);

            logger.LogInformation("Received new scan result for storage {AccountName}", blobUriBuilder.AccountName);

            if (blobUriBuilder.BlobContainerName != AzureSdkClientConstants.MainContainer)
            {
                logger.LogInformation("Event is not from the interested containers, ignoring");
                return;
            }

            var blobInfo = ParseBlobInfoFromUri(blobUri);

            if (string.IsNullOrWhiteSpace(blobInfo.FilePath) || string.IsNullOrWhiteSpace(blobInfo.FileName))
            {
                logger.LogError("BlobInfo contains null or empty FilePath or FileName for blob {BlobUri}", blobUri);
                throw new ArgumentException("BlobInfo contains null or empty FilePath or FileName.");
            }

            if (verdict == MaliciousVerdict || verdict == FailedVerdict)
            {
                try
                {
                    logger.LogInformation("blob {0} is {1}, deleting it from {2} container", blobInfo.FileName, verdict, AzureSdkClientConstants.MainContainer);
                    //Restore this with RPLAT-5668 - await repository.RemoveBlobAsync(AzureSdkClientConstants.MainContainer, blobInfo.FilePath, blobInfo.FileName);
                    return;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error deleting blob {0} with verdict {1}", blobUri, verdict);
                    throw;
                }
            }

            if (verdict == CleanVerdict)
            {
                logger.LogInformation("blob {0} is clean, beginning post-processing", blobUri);
                try
                {
                    //send lesson content to Rustici
                    logger.LogInformation("sending blob {0} to Rustici", blobUri);

                    //build lesson update var updatedLessonDto = await BuildLessonDto(blobInfo);

                    //update lesson
                    logger.LogInformation("updating lesson associated with blob {0}", blobUri);
                    //probably create a new method in LearningContentService to handle this that encapsulates the following
                    //var patch = new JsonPatchDocument();
                    //patch.Replace($"/sections/{sectionIndex}/learningObjects/{lessonIndex}", newLesson);
                    //await UpdateLearningContentAsync(courseId, patch, existingLearningContentDto, cancellationToken);
                    return;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An issue occured while completing post-processing tasks'{0}'", blobUri);
                    throw;
                }
            }
        }

        //private async Task<UpdateLessonDto> BuildLessonDto(BlobInfo blobInfo)
        //{
        //    // Get existing lesson by courseId and lessonId
        //    var learningContent = await learningContentService.GetByCourseIdAsync(blobInfo.CourseId, CancellationToken.None);

        //    if (learningContent == null)
        //    {
        //        throw new InvalidOperationException($"Learning content for course {Guid.Empty} not found.");
        //    }

        //    var existingLessonDto = learningContent.Sections
        //        .SelectMany(section => section.LearningObjects)
        //        .OfType<LessonDto>()
        //        .FirstOrDefault(lesson => lesson.LearningObjectId == blobInfo.LessonId);


        //    if (existingLessonDto == null)
        //    {
        //        throw new InvalidOperationException($"Lesson with ID {blobInfo.LessonId} not found in course {blobInfo.CourseId}.");
        //    }

        //    //Create new LessonDto based on existing lesson with updated contentStatus
        //    var updatedLessonDto = new LessonDto
        //    {
        //        LearningObjectType = LearningObjectType.Lesson,
        //        LearningObjectId = existingLessonDto.LearningObjectId,
        //        LessonType = existingLessonDto.LessonType,
        //        Name = existingLessonDto.Name,
        //        DurationMinutes = existingLessonDto.DurationMinutes,
        //        RequiredForCompletion = existingLessonDto.RequiredForCompletion,
        //        RequiresAudio = existingLessonDto.RequiresAudio,
        //        RequiresVideo = existingLessonDto.RequiresVideo,
        //        OpensInNewTab = existingLessonDto.OpensInNewTab,
        //        ContentPath = blobInfo.FilePath,
        //        FileName = blobInfo.FileName,
        //        ContentStatus = LessonContentStatusDto.Ready,
        //    };
        //}

        //private enum LessonContentStatusDto
        //{
        //    Pending = 1,
        //    Failed = 2,
        //    Malicious = 3,
        //    Ready = 4
        //}

        private BlobMetadata ParseBlobInfoFromUri(Uri blobUri)
        {
            //expected Uri format "https://mystorageaccount.blob.core.windows.net/main/8/courseIdGuid/lessonIdGuid/fileName.ext"

            var segments = blobUri.AbsolutePath.TrimStart('/').Split('/');
            if (segments.Length < 5)
            {
                throw new ArgumentException($"Invalid blob URI format: {blobUri}");
            }

            if (!int.TryParse(segments[1], out var orgId))
                throw new ArgumentException($"Invalid OrgId in blob URI: {blobUri}");

            if (!Guid.TryParse(segments[2], out var courseId))
                throw new ArgumentException($"Invalid CourseId in blob URI: {blobUri}");

            if (!Guid.TryParse(segments[3], out var lessonId))
                throw new ArgumentException($"Invalid LessonId in blob URI: {blobUri}");

            if (blobUri.LocalPath.LastIndexOf('/') == -1)
            {
                throw new ArgumentException($"Invalid blob URI format, missing '/' in path: {blobUri}");
            }

            return new BlobMetadata
            {
                ContainerName = segments[0],
                OrgId = orgId,
                CourseId = courseId,
                LessonId = lessonId,
                FilePath = blobUri.LocalPath.Substring(0, blobUri.LocalPath.LastIndexOf('/')).Replace($"/{AzureSdkClientConstants.MainContainer}/", string.Empty),
                FileName = segments[4]
            };
        }
    }
}
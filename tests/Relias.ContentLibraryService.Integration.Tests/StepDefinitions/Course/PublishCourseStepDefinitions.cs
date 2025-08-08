using Relias.ContentLibraryService.Integration.Tests.Context;
using Relias.ContentLibraryService.Integration.Tests.Utilities;
using Reqnroll;
using Relias.ContentLibraryService.Infra.Persistence;
using Relias.ContentLibraryService.Common.Enums;
using Relias.ContentLibraryService.App.Features.Course.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Relias.ContentLibraryService.Integration.Tests.StepDefinitions.Course
{
    [Binding]
    public class PublishCourseStepDefinitions(
        HttpResponseContext httpResponseContext,
        AuthenticationHeaderContext authenticationHeaderContext,
        ScenarioService scenarioService)
    {
        private const string EndpointUri = "api/v1/courses";
        private readonly HttpClient _client = scenarioService.ScenarioHttpClient ?? throw new ArgumentNullException(nameof(scenarioService.ScenarioHttpClient));
        private readonly Guid _validCourseId = GlobalTestSetup.PublishCourseIds[0];
        private readonly Guid _validUserId = Guid.NewGuid();
        private readonly DateTime _currentTime = DateTime.UtcNow;
        private readonly DateTime _scheduledPublishTime = DateTime.UtcNow.AddDays(7);
        private readonly CancellationToken _cancellationToken = new();
        private readonly ApplicationDbContext _context = GlobalTestSetup.CreateContext();

        [BeforeStep]
        public void SetDefaultAuthorizationHeader()
        {
            if (authenticationHeaderContext.AuthenticationHeader is not null)
            {
                _client.DefaultRequestHeaders.Authorization = authenticationHeaderContext.AuthenticationHeader;
            }
        }

        [Given("all the required fields to be filled out to publish or schedule publish a course")]
        public void GivenAllTheRequiredFieldsToBeFilledOutToPublishOrSchedulePublishACourse()
        {
            var course = _context.Courses.Find(_validCourseId);
            Assert.NotNull(course);
            Assert.False(string.IsNullOrWhiteSpace(course.Title));
            Assert.False(string.IsNullOrWhiteSpace(course.BriefDescription));
            
            var random = new Random();
            var statuses = new[] { (int)StatusIdEnums.Draft, (int)StatusIdEnums.ScheduledForPublish };
            course.StatusId = (byte)statuses[random.Next(statuses.Length)];
        }


        [When("the user publishes a course immediately")]
        public async Task WhenTheUserPublishesACourseImmediately()
        {
            var publishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishDate = _currentTime,
                PublishBy = _validUserId.ToString()
            };

            var requestContent = HttpExtensions.CreateRequestBody(publishCourseDto);

            httpResponseContext.Response = await _client.PostAsync($"{EndpointUri}/{_validCourseId}/publish", requestContent, _cancellationToken);
        }

        [When("the user schedules a course to be published for a future date and time")]
        public async Task WhenTheUserSchedulesACourseToBePublishedForAFutureDateAndTime()
        {
            var publishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.ScheduledForPublish,
                PublishDate = _scheduledPublishTime,
                PublishBy = _validUserId.ToString()
            };

            var requestContent = HttpExtensions.CreateRequestBody(publishCourseDto);

            httpResponseContext.Response = await _client.PostAsync($"{EndpointUri}/{_validCourseId}/publish", requestContent, _cancellationToken);
        }

        [Then("the request should be successful")]
        public void ThenTheRequestShouldBeSuccessful()
        {
            var response = httpResponseContext!.Response!.StatusCode;
            Assert.Equal(HttpStatusCode.NoContent, response);
        }

        [Then("the course should be marked as published with the current dateTime")]
        public async Task ThenTheCourseShouldBeMarkedAsPublishedWithTheCurrentDateTime()
        {
            var course = await _context.Courses.FindAsync(_validCourseId);

            Assert.NotNull(course);
            Assert.Equal((int)StatusIdEnums.Published, course.StatusId);

            var timeDifference = Math.Abs((course.PublishDate!.Value - _currentTime).TotalSeconds);
            Assert.True(timeDifference <= 5);
        }

        [Then("the course should be marked as scheduled for publish with a future date and time")]
        public async Task ThenTheCourseShouldBeMarkedAsScheduledForPublishWithAFutureDateAndTime()
        {
            var course = await _context.Courses.FindAsync(_validCourseId);
            Assert.NotNull(course);
            Assert.Equal((int)StatusIdEnums.ScheduledForPublish, course.StatusId);

            var scheduledCourse = await _context.ScheduledCourses.FirstOrDefaultAsync(sc => sc.CourseId == _validCourseId, _cancellationToken);
            Assert.NotNull(scheduledCourse);
            Assert.Equal(_scheduledPublishTime, scheduledCourse.ScheduledDate);
        }

        [When("the user tries to publish a course that does not exist")]
        public void WhenTheUserTriesToPublishACourseThatDoesNotExist()
        {
            var invalidCourseId = Guid.NewGuid();
            var publishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishDate = _currentTime,
                PublishBy = _validUserId.ToString()
            };

            var requestContent = HttpExtensions.CreateRequestBody(publishCourseDto);

            httpResponseContext.Response = _client.PostAsync($"{EndpointUri}/{invalidCourseId}/publish", requestContent, _cancellationToken).Result;
        }

        [When("the user tries to schedule a course with a past date")]
        public void WhenTheUserTriesToScheduleACourseWithAPastDate()
        {
            var publishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.ScheduledForPublish,
                PublishDate = DateTime.UtcNow.AddDays(-1), // Past date
                PublishBy = _validUserId.ToString()
            };

            var requestContent = HttpExtensions.CreateRequestBody(publishCourseDto);

            httpResponseContext.Response = _client.PostAsync($"{EndpointUri}/{_validCourseId}/publish", requestContent, _cancellationToken).Result;
        }

        [When("the user tries to publish a course that is already published")]
        public async Task WhenTheUserTriesToPublishACourseThatIsAlreadyPublished()
        {
            var publishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishDate = _currentTime,
                PublishBy = _validUserId.ToString()
            };

            var requestContent = HttpExtensions.CreateRequestBody(publishCourseDto);

            httpResponseContext.Response = await _client.PostAsync($"{EndpointUri}/{_validCourseId}/publish", requestContent, _cancellationToken);

            var course = await _context.Courses.FindAsync(_validCourseId);
            Assert.NotNull(course);
            Assert.Equal((int)StatusIdEnums.Published, course.StatusId);

            var newPublishCourseDto = new PublishCourseDto
            {
                StatusId = (int)StatusIdEnums.Published,
                PublishDate = _currentTime.AddMinutes(5),
                PublishBy = _validUserId.ToString()
            };

            var newRequestContent = HttpExtensions.CreateRequestBody(newPublishCourseDto);
            httpResponseContext.Response = await _client.PostAsync($"{EndpointUri}/{_validCourseId}/publish", newRequestContent, _cancellationToken);
        }
    }
}

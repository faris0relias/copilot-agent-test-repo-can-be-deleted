using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;

namespace Relias.ContentLibraryService.App.Tests.Features.Course.Commands
{
    public class UpdateFinalExamCommandHandlerTests
    {
        private readonly Mock<IFinalExamService> _serviceMock = new();
        private readonly UpdateFinalExamCommand.Handler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public UpdateFinalExamCommandHandlerTests()
        {
            _handler = new UpdateFinalExamCommand.Handler(_serviceMock.Object);
        }
        [Fact]
        public async Task Handle_ShouldReturnFinalExamDto_WhenFinalExamIsUpdatedSuccessfully()
        {
            var courseId = Guid.NewGuid();
            int organizationId = 1;
            var updatedFinalExamDto = new FinalExamDto()
            {
                MinimumPercentageToPass = 40
            };
            var providedValues = new Dictionary<string, dynamic>()
            {
                {"MinimumPercentageToPass", 40}
            };
            var expectedFinalExamDto = new FinalExamDto
            {
                MinimumPercentageToPass = 40
            };

            _serviceMock
                .Setup(service => service.UpdateFinalExamAsync(courseId, organizationId, providedValues, _cancellationToken))
                .ReturnsAsync(expectedFinalExamDto);

            var contract = new UpdateFinalExamCommand.Contract
            {
                CourseId = courseId,
                OrganizationId = organizationId,
                UpdatedValues = providedValues,
                UpdatedFinalExam = updatedFinalExamDto
            };

            var result = await _handler.Handle(contract, _cancellationToken);

            Assert.NotNull(result);
            Assert.Equal(expectedFinalExamDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
            _serviceMock.Verify(service => service.UpdateFinalExamAsync(courseId, organizationId, providedValues, _cancellationToken), Times.Once);
        }
    }
}
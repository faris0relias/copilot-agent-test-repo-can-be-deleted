using Moq;
using Relias.ContentLibraryService.App.Features.Course.Commands;
using Relias.ContentLibraryService.App.Features.Course.Dtos.FinalExam;
using Relias.ContentLibraryService.App.Interfaces;
using Relias.ContentLibraryService.Common.Exceptions;

namespace Relias.ContentLibraryService.Unit.Tests.Features.Course.Commands
{
    public class CreateFinalExamCommandHandlerTests
    {
        private readonly Mock<IFinalExamService> _serviceMock = new();
        private readonly CreateFinalExamCommand.Handler _handler;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public CreateFinalExamCommandHandlerTests()
        {
            _handler = new CreateFinalExamCommand.Handler(_serviceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFinalExamDto_WhenFinalExamIsCreatedSuccessfully()
        {
            var courseId = Guid.NewGuid();
            int organizationId = 1;
            var expectedFinalExamDto = new FinalExamDto
            {
                MinimumPercentageToPass = null
            };

            _serviceMock
                .Setup(service => service.CreateFinalExamAsync(courseId, organizationId, _cancellationToken))
                .ReturnsAsync(expectedFinalExamDto);

            var contract = new CreateFinalExamCommand.Contract
            {
                CourseId = courseId,
                OrganizationId = organizationId
            };
            var result = await _handler.Handle(contract, _cancellationToken);

            Assert.NotNull(result);
            Assert.Equal(expectedFinalExamDto.MinimumPercentageToPass, result.MinimumPercentageToPass);
            _serviceMock.Verify(service => service.CreateFinalExamAsync(courseId, organizationId, _cancellationToken), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowError_WhenFinalExamCreationFails()
        {
            var courseId = Guid.NewGuid();
            int organizationId = 1;

            _serviceMock
                .Setup(service => service.CreateFinalExamAsync(courseId, organizationId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new BadRequestException($"You can't add final exam. Course with id {courseId} does not exist"));

            var contract = new CreateFinalExamCommand.Contract
            {
                CourseId = courseId,
                OrganizationId = organizationId
            };

            await Assert.ThrowsAsync<BadRequestException>(() => _handler.Handle(contract, _cancellationToken)); _serviceMock.Verify(service => service.CreateFinalExamAsync(courseId, organizationId, _cancellationToken), Times.Once);
        }
    }
}
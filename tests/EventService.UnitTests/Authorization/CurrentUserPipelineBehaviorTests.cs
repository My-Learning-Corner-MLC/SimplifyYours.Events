using EventService.Application.Authorization;
using EventService.Application.Events.CreateEvent;
using MediatR;
using Moq;

namespace EventService.UnitTests.Authorization;

public sealed class CurrentUserPipelineBehaviorTests
{
    [Fact]
    public async Task Handle_WhenRequestIsBaseCommand_HydratesCurrentUserFromAccessor()
    {
        var expectedUser = new CurrentUser(Guid.NewGuid(), Guid.NewGuid());
        var accessor = new Mock<ICurrentUserAccessor>();
        accessor.SetupGet(a => a.User).Returns(expectedUser);
        var behavior = new CurrentUserPipelineBehavior<CreateEventCommand, CreateEventResult>(accessor.Object);
        var command = new CreateEventCommand("Plan", null, "event", null);
        var nextCalled = false;

        await behavior.Handle(
            command,
            () =>
            {
                nextCalled = true;
                return Task.FromResult<CreateEventResult>(null!);
            },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal(expectedUser, command.CurrentUser);
    }

    [Fact]
    public async Task Handle_WhenRequestIsBaseCommand_AndAccessorReturnsNull_Throws()
    {
        var accessor = new Mock<ICurrentUserAccessor>();
        accessor.SetupGet(a => a.User).Returns((CurrentUser?)null);
        var behavior = new CurrentUserPipelineBehavior<CreateEventCommand, CreateEventResult>(accessor.Object);
        var command = new CreateEventCommand("Plan", null, "event", null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(
            command,
            () => Task.FromResult<CreateEventResult>(null!),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequestIsNotBaseCommand_PassesThroughWithoutTouchingAccessor()
    {
        var accessor = new Mock<ICurrentUserAccessor>(MockBehavior.Strict);
        var behavior = new CurrentUserPipelineBehavior<NonBaseRequest, Unit>(accessor.Object);
        var request = new NonBaseRequest();
        var nextCalled = false;

        await behavior.Handle(
            request,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Unit.Value);
            },
            CancellationToken.None);

        Assert.True(nextCalled);
        accessor.VerifyGet(a => a.User, Times.Never);
    }

    private sealed record NonBaseRequest : IRequest<Unit>;
}

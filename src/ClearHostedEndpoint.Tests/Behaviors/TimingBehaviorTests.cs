using ClearMeasure.HostedEndpoint.Behaviors;
using FluentAssertions;
using Moq;
using NServiceBus.Pipeline;
using Serilog;
using Xunit;

namespace ClearHostedEndpoint.Tests.Behaviors;

public class TimingBehaviorTests
{
    [Fact]
    public void Constructor_WithValidLogger_ShouldNotThrow()
    {
        var logger = Mock.Of<ILogger>();

        var act = () => new TimingBehavior(logger);

        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        var act = () => new TimingBehavior(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Invoke_ShouldLogStartAndCompletion()
    {
        var loggerMock = new Mock<ILogger>();
        var behavior = new TimingBehavior(loggerMock.Object);
        var context = CreateMockContext();
        var nextCalled = false;

        await behavior.Invoke(context, () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        nextCalled.Should().BeTrue();

        loggerMock.Verify(
            l => l.Information(
                It.Is<string>(s => s.Contains("starting to process")),
                It.IsAny<object[]>()),
            Times.Once);

        loggerMock.Verify(
            l => l.Information(
                It.Is<string>(s => s.Contains("completed processing")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_WhenHandlerThrows_ShouldLogErrorAndRethrow()
    {
        var loggerMock = new Mock<ILogger>();
        var behavior = new TimingBehavior(loggerMock.Object);
        var context = CreateMockContext();
        var expectedException = new InvalidOperationException("Test exception");

        var act = async () => await behavior.Invoke(context, () => throw expectedException);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        loggerMock.Verify(
            l => l.Error(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("failed processing")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldCaptureElapsedTime()
    {
        var loggerMock = new Mock<ILogger>();
        var behavior = new TimingBehavior(loggerMock.Object);
        var context = CreateMockContext();

        await behavior.Invoke(context, async () =>
        {
            await Task.Delay(50);
        });

        loggerMock.Verify(
            l => l.Information(
                It.Is<string>(s => s.Contains("completed processing") && s.Contains("ElapsedMilliseconds")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldIncludeHandlerAndMessageTypeInLogs()
    {
        var loggerMock = new Mock<ILogger>();
        var behavior = new TimingBehavior(loggerMock.Object);
        var context = CreateMockContext();

        await behavior.Invoke(context, () => Task.CompletedTask);

        loggerMock.Verify(
            l => l.Information(
                It.Is<string>(s => s.Contains("HandlerType") && s.Contains("MessageType")),
                It.IsAny<object[]>()),
            Times.AtLeastOnce);
    }

    private static IInvokeHandlerContext CreateMockContext()
    {
        var message = new TestMessage();
        var handler = new TestHandler();

        var messageHandlerMock = new Mock<MessageHandler>();
        messageHandlerMock.Setup(h => h.Instance).Returns(handler);

        var contextMock = new Mock<IInvokeHandlerContext>();
        contextMock.Setup(c => c.MessageBeingHandled).Returns(message);
        contextMock.Setup(c => c.MessageHandler).Returns(messageHandlerMock.Object);

        return contextMock.Object;
    }

    private class TestMessage
    {
    }

    private class TestHandler
    {
    }
}

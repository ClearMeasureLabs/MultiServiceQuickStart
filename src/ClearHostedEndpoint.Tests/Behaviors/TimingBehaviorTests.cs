using ClearMeasure.HostedEndpoint.Behaviors;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus.Pipeline;
using Xunit;

namespace ClearHostedEndpoint.Tests.Behaviors;

public class TimingBehaviorTests
{
    [Fact]
    public void Constructor_WhenLoggerIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new TimingBehavior(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Invoke_WhenHandlerSucceeds_ShouldLogInformationWithDuration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TimingBehavior>>();
        var behavior = new TimingBehavior(mockLogger.Object);
        
        var mockContext = new Mock<IInvokeHandlerContext>();
        var testMessage = new TestMessage();
        var mockHandlerInstance = new Mock<object>();
        var mockHandler = new Mock<MessageHandler>();
        mockHandler.Setup(x => x.Instance).Returns(mockHandlerInstance.Object);
        
        mockContext.Setup(x => x.MessageBeingHandled).Returns(testMessage);
        mockContext.Setup(x => x.MessageId).Returns("test-message-id-123");
        mockContext.Setup(x => x.MessageHandler).Returns(mockHandler.Object);
        
        var handlerInvoked = false;
        Task Next() 
        {
            handlerInvoked = true;
            return Task.CompletedTask;
        }

        // Act
        await behavior.Invoke(mockContext.Object, Next);

        // Assert
        handlerInvoked.Should().BeTrue();
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message handler completed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_WhenHandlerFails_ShouldLogErrorWithDuration()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TimingBehavior>>();
        var behavior = new TimingBehavior(mockLogger.Object);
        
        var mockContext = new Mock<IInvokeHandlerContext>();
        var testMessage = new TestMessage();
        var mockHandlerInstance = new Mock<object>();
        var mockHandler = new Mock<MessageHandler>();
        mockHandler.Setup(x => x.Instance).Returns(mockHandlerInstance.Object);
        
        mockContext.Setup(x => x.MessageBeingHandled).Returns(testMessage);
        mockContext.Setup(x => x.MessageId).Returns("test-message-id-456");
        mockContext.Setup(x => x.MessageHandler).Returns(mockHandler.Object);
        
        var expectedException = new InvalidOperationException("Test error");
        Task Next() => throw expectedException;

        // Act
        var act = async () => await behavior.Invoke(mockContext.Object, Next);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test error");
            
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Message handler failed")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldIncludeMessageTypeInLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TimingBehavior>>();
        var behavior = new TimingBehavior(mockLogger.Object);
        
        var mockContext = new Mock<IInvokeHandlerContext>();
        var testMessage = new TestMessage();
        var mockHandlerInstance = new Mock<object>();
        var mockHandler = new Mock<MessageHandler>();
        mockHandler.Setup(x => x.Instance).Returns(mockHandlerInstance.Object);
        
        mockContext.Setup(x => x.MessageBeingHandled).Returns(testMessage);
        mockContext.Setup(x => x.MessageId).Returns("test-message-id-789");
        mockContext.Setup(x => x.MessageHandler).Returns(mockHandler.Object);
        
        Task Next() => Task.CompletedTask;

        // Act
        await behavior.Invoke(mockContext.Object, Next);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestMessage")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldIncludeMessageIdInLog()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TimingBehavior>>();
        var behavior = new TimingBehavior(mockLogger.Object);
        
        const string messageId = "unique-test-message-id";
        var mockContext = new Mock<IInvokeHandlerContext>();
        var testMessage = new TestMessage();
        var mockHandlerInstance = new Mock<object>();
        var mockHandler = new Mock<MessageHandler>();
        mockHandler.Setup(x => x.Instance).Returns(mockHandlerInstance.Object);
        
        mockContext.Setup(x => x.MessageBeingHandled).Returns(testMessage);
        mockContext.Setup(x => x.MessageId).Returns(messageId);
        mockContext.Setup(x => x.MessageHandler).Returns(mockHandler.Object);
        
        Task Next() => Task.CompletedTask;

        // Act
        await behavior.Invoke(mockContext.Object, Next);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_ShouldMeasureExecutionTime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TimingBehavior>>();
        var behavior = new TimingBehavior(mockLogger.Object);
        
        var mockContext = new Mock<IInvokeHandlerContext>();
        var testMessage = new TestMessage();
        var mockHandlerInstance = new Mock<object>();
        var mockHandler = new Mock<MessageHandler>();
        mockHandler.Setup(x => x.Instance).Returns(mockHandlerInstance.Object);
        
        mockContext.Setup(x => x.MessageBeingHandled).Returns(testMessage);
        mockContext.Setup(x => x.MessageId).Returns("timing-test-message");
        mockContext.Setup(x => x.MessageHandler).Returns(mockHandler.Object);
        
        async Task Next()
        {
            await Task.Delay(50); // Simulate work
        }

        // Act
        await behavior.Invoke(mockContext.Object, Next);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Duration=")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestMessage
    {
        public string Data { get; set; } = "test";
    }
}

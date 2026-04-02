using ClearMeasure.HostedEndpoint.Infrastructure;
using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Moq;
using NServiceBus.Pipeline;
using Xunit;

namespace ClearHostedEndpoint.Tests.Infrastructure;

public class HandlerTimingBehaviorTests
{
    [Fact]
    public async Task Invoke_ShouldCallNextBehavior()
    {
        // Arrange
        var behavior = new HandlerTimingBehavior();
        var context = new Mock<IInvokeHandlerContext>();
        var messageHandler = new Mock<MessageHandler>();
        messageHandler.Setup(h => h.HandlerType).Returns(typeof(HandlerTimingBehaviorTests));
        context.Setup(c => c.MessageHandler).Returns(messageHandler.Object);
        context.Setup(c => c.MessageBeingHandled).Returns(new object());
        context.Setup(c => c.MessageId).Returns("test-id");
        context.Setup(c => c.Headers).Returns(new Dictionary<string, string>());
        
        var nextCalled = false;

        // Act
        await behavior.Invoke(context.Object, () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WhenTelemetryClientIsNull_ShouldNotThrow()
    {
        // Arrange
        var behavior = new HandlerTimingBehavior(null);
        var context = new Mock<IInvokeHandlerContext>();
        var messageHandler = new Mock<MessageHandler>();
        messageHandler.Setup(h => h.HandlerType).Returns(typeof(HandlerTimingBehaviorTests));
        context.Setup(c => c.MessageHandler).Returns(messageHandler.Object);
        context.Setup(c => c.MessageBeingHandled).Returns(new object());
        context.Setup(c => c.MessageId).Returns("test-id");
        context.Setup(c => c.Headers).Returns(new Dictionary<string, string>());

        // Act
        Func<Task> act = async () => await behavior.Invoke(context.Object, () => Task.CompletedTask);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Invoke_WhenExceptionOccurs_ShouldRethrowException()
    {
        // Arrange
        var behavior = new HandlerTimingBehavior();
        var context = new Mock<IInvokeHandlerContext>();
        var messageHandler = new Mock<MessageHandler>();
        messageHandler.Setup(h => h.HandlerType).Returns(typeof(HandlerTimingBehaviorTests));
        context.Setup(c => c.MessageHandler).Returns(messageHandler.Object);
        context.Setup(c => c.MessageBeingHandled).Returns(new object());
        context.Setup(c => c.MessageId).Returns("test-id");
        context.Setup(c => c.Headers).Returns(new Dictionary<string, string>());
        
        var expectedException = new InvalidOperationException("Test exception");

        // Act
        Func<Task> act = async () => await behavior.Invoke(context.Object, () => throw expectedException);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");
    }

    [Fact]
    public async Task Invoke_WithTelemetryClient_ShouldTrackDependency()
    {
        // Arrange
        var telemetryItems = new List<ITelemetry>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = new StubTelemetryChannel { OnSend = telemetryItems.Add }
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var behavior = new HandlerTimingBehavior(telemetryClient);
        
        var context = new Mock<IInvokeHandlerContext>();
        var messageHandler = new Mock<MessageHandler>();
        messageHandler.Setup(h => h.HandlerType).Returns(typeof(HandlerTimingBehaviorTests));
        context.Setup(c => c.MessageHandler).Returns(messageHandler.Object);
        context.Setup(c => c.MessageBeingHandled).Returns(new object());
        context.Setup(c => c.MessageId).Returns("test-id");
        context.Setup(c => c.Headers).Returns(new Dictionary<string, string>());

        // Act
        await behavior.Invoke(context.Object, () => Task.CompletedTask);

        // Give telemetry time to flush
        await Task.Delay(100);

        // Assert
        telemetryItems.Should().NotBeEmpty();
        telemetryItems.Should().ContainSingle(t => t is DependencyTelemetry);
        
        var dependency = telemetryItems.OfType<DependencyTelemetry>().First();
        dependency.Type.Should().Be("NServiceBus.Handler");
        dependency.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Invoke_WhenExceptionOccurs_ShouldTrackFailedDependency()
    {
        // Arrange
        var telemetryItems = new List<ITelemetry>();
        var telemetryConfiguration = new TelemetryConfiguration
        {
            TelemetryChannel = new StubTelemetryChannel { OnSend = telemetryItems.Add }
        };
        var telemetryClient = new TelemetryClient(telemetryConfiguration);
        var behavior = new HandlerTimingBehavior(telemetryClient);
        
        var context = new Mock<IInvokeHandlerContext>();
        var messageHandler = new Mock<MessageHandler>();
        messageHandler.Setup(h => h.HandlerType).Returns(typeof(HandlerTimingBehaviorTests));
        context.Setup(c => c.MessageHandler).Returns(messageHandler.Object);
        context.Setup(c => c.MessageBeingHandled).Returns(new object());
        context.Setup(c => c.MessageId).Returns("test-id");
        context.Setup(c => c.Headers).Returns(new Dictionary<string, string>());
        
        var expectedException = new InvalidOperationException("Handler failed");

        // Act
        try
        {
            await behavior.Invoke(context.Object, () => throw expectedException);
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Give telemetry time to flush
        await Task.Delay(100);

        // Assert
        telemetryItems.Should().NotBeEmpty();
        telemetryItems.Should().ContainSingle(t => t is DependencyTelemetry);
        
        var dependency = telemetryItems.OfType<DependencyTelemetry>().First();
        dependency.Type.Should().Be("NServiceBus.Handler");
        dependency.Success.Should().BeFalse();
        dependency.Properties.Should().ContainKey("ExceptionType");
        dependency.Properties.Should().ContainKey("ExceptionMessage");
    }

    private class StubTelemetryChannel : ITelemetryChannel
    {
        public Action<ITelemetry>? OnSend { get; set; }
        public bool? DeveloperMode { get; set; }
        public string? EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            OnSend?.Invoke(item);
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }
}

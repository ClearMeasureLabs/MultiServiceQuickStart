using ClearMeasure.HostedService.Exceptions;

using FluentAssertions;
using Xunit;

namespace QuickHostedService.Tests.Core;

public class HostedServiceExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new HostedServiceException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Test error message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new HostedServiceException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}

public class ServiceRegistrationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Registration failed";

        // Act
        var exception = new ServiceRegistrationException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_ShouldInheritFromHostedServiceException()
    {
        // Arrange & Act
        var exception = new ServiceRegistrationException("Test");

        // Assert
        exception.Should().BeAssignableTo<HostedServiceException>();
    }
}

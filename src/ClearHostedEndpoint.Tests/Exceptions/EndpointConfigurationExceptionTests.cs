namespace ClearHostedEndpoint.Tests.Exceptions;

/// <summary>
/// Unit tests for EndpointConfigurationException.
/// </summary>
public class EndpointConfigurationExceptionTests
{
    [Fact]
    public void Constructor_WithNoParameters_ShouldCreateException()
    {
        // Act
        var exception = new EndpointConfigurationException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var exception = new EndpointConfigurationException(message);

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
        var exception = new EndpointConfigurationException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Exception_ShouldBeThrowable()
    {
        // Arrange
        var message = "Test error";

        // Act & Assert
        Action act = () => throw new EndpointConfigurationException(message);
        act.Should().Throw<EndpointConfigurationException>()
            .WithMessage(message);
    }

    [Fact]
    public void Exception_ShouldBeInstanceOfException()
    {
        // Act
        var exception = new EndpointConfigurationException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Exception_WithInnerException_ShouldPreserveStackTrace()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Outer error";

        // Act
        var exception = new EndpointConfigurationException(message, innerException);

        // Assert
        exception.InnerException.Should().NotBeNull();
        exception.InnerException.Should().BeSameAs(innerException);
        exception.InnerException!.Message.Should().Be("Inner error");
    }

    [Fact]
    public void Exception_CanBeCaught_AsBaseException()
    {
        // Arrange
        Exception? caughtException = null;

        // Act
        try
        {
            throw new EndpointConfigurationException("Test");
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        caughtException.Should().NotBeNull();
        caughtException.Should().BeOfType<EndpointConfigurationException>();
    }

    [Fact]
    public void Exception_WithNullMessage_ShouldNotThrow()
    {
        // Act
        var act = () => new EndpointConfigurationException(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Exception_WithEmptyMessage_ShouldNotThrow()
    {
        // Act
        var act = () => new EndpointConfigurationException(string.Empty);

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Simple message")]
    [InlineData("Message with special chars: !@#$%^&*()")]
    [InlineData("Multi\nline\nmessage")]
    public void Exception_WithVariousMessages_ShouldPreserveMessage(string message)
    {
        // Act
        var exception = new EndpointConfigurationException(message);

        // Assert
        exception.Message.Should().Be(message);
    }
}

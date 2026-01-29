namespace ClearMeasure.HostedEndpoint.Exceptions;

/// <summary>
/// Exception thrown when there is an error configuring or starting the NServiceBus endpoint.
/// </summary>
public class EndpointConfigurationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointConfigurationException"/> class.
    /// </summary>
    public EndpointConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointConfigurationException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EndpointConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointConfigurationException"/> class
    /// with a specified error message and a reference to the inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EndpointConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

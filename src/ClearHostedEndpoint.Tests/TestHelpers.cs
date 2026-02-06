namespace ClearHostedEndpoint.Tests;

/// <summary>
/// Helper methods for tests.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates an empty test configuration.
    /// </summary>
    public static IConfiguration CreateTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();
    }
}

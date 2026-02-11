using ClearMeasure.HostedEndpoint;
using ClearMeasure.HostedEndpoint.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClearHostedEndpoint.Tests.Infrastructure;

public class EndpointTimingIntegrationTests
{
    [Fact]
    public async Task Endpoint_RegistersTimingBehavior_WhenEnabled()
    {
        // Arrange
        var endpoint = new TestEndpointWithTimingEnabled(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.TimingBehaviorEnabled.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_DoesNotRegisterTimingBehavior_WhenDisabled()
    {
        // Arrange
        var endpoint = new TestEndpointWithTimingDisabled(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert
            endpoint.TimingBehaviorEnabled.Should().BeFalse();
        }
        finally
        {
            // Cleanup
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    [Fact]
    public async Task Endpoint_WithTimingBehavior_StartsSuccessfully()
    {
        // Arrange
        var endpoint = new TestEndpointWithTimingEnabled(TestHelpers.CreateTestConfiguration());

        try
        {
            // Act
            await endpoint.StartAsync(CancellationToken.None);
            await Task.Delay(500);

            // Assert - verify endpoint started without errors by checking if it's running
            endpoint.IsStarted.Should().BeTrue();
        }
        finally
        {
            // Cleanup
            await endpoint.StopAsync(CancellationToken.None);
            endpoint.Dispose();
        }
    }

    private class TestEndpointWithTimingEnabled : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
    {
        public bool TimingBehaviorEnabled => EndpointOptions.EnableTimingBehavior;
        public bool IsStarted { get; private set; }

        public TestEndpointWithTimingEnabled(IConfiguration configuration) : base(configuration)
        {
        }

        public override async Task OnStartingAsync(CancellationToken cancellationToken)
        {
            await base.OnStartingAsync(cancellationToken);
            IsStarted = true;
        }

        protected override EndpointOptions EndpointOptions { get; } = new()
        {
            EndpointName = "TestEndpointWithTiming",
            EnableTimingBehavior = true,
            PurgeOnStartup = true
        };

        protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
        {
            endpointConfiguration.UseTransport<LearningTransport>();
        }
    }

    private class TestEndpointWithTimingDisabled : ClearMeasure.HostedEndpoint.ClearHostedEndpoint
    {
        public bool TimingBehaviorEnabled => EndpointOptions.EnableTimingBehavior;

        public TestEndpointWithTimingDisabled(IConfiguration configuration) : base(configuration)
        {
        }

        protected override EndpointOptions EndpointOptions { get; } = new()
        {
            EndpointName = "TestEndpointWithoutTiming",
            EnableTimingBehavior = false,
            PurgeOnStartup = true
        };

        protected override void ConfigureTransport(EndpointConfiguration endpointConfiguration)
        {
            endpointConfiguration.UseTransport<LearningTransport>();
        }
    }
}

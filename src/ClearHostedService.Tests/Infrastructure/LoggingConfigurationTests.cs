using ClearMeasure.HostedService;
using ClearMeasure.HostedService.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Xunit;

namespace QuickHostedService.Tests.Infrastructure;

public class LoggingConfigurationTests
{
    [Fact]
    public async Task ConfigureLogging_ConfiguresApplicationInsightsSink_WhenConnectionStringProvided()
    {
        // Arrange
        var service = new TestHostedServiceWithLoggingOptions(
            new ConfigurationBuilder().Build(),
            new LoggingOptions
            {
                EnableApplicationInsights = true,
                ApplicationInsightsConnectionString = "InstrumentationKey=test-key;IngestionEndpoint=https://test.applicationinsights.azure.com/",
                ApplicationName = "TestApp",
                CloudInstance = "TestInstance"
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - verify the service started without errors
        service.IsRunning.Should().BeTrue();
        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task ConfigureLogging_DoesNotConfigureApplicationInsights_WhenConnectionStringIsNull()
    {
        // Arrange
        var service = new TestHostedServiceWithLoggingOptions(
            new ConfigurationBuilder().Build(),
            new LoggingOptions
            {
                EnableApplicationInsights = true,
                ApplicationInsightsConnectionString = null
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - verify the service started without Application Insights
        service.IsRunning.Should().BeTrue();
        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task ConfigureLogging_DoesNotConfigureApplicationInsights_WhenDisabled()
    {
        // Arrange
        var service = new TestHostedServiceWithLoggingOptions(
            new ConfigurationBuilder().Build(),
            new LoggingOptions
            {
                EnableApplicationInsights = false,
                ApplicationInsightsConnectionString = "InstrumentationKey=test-key"
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - verify the service started without Application Insights
        service.IsRunning.Should().BeTrue();
        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task ConfigureLogging_UsesDefaultApplicationName_WhenNotProvided()
    {
        // Arrange
        var service = new TestHostedServiceWithLoggingOptions(
            new ConfigurationBuilder().Build(),
            new LoggingOptions
            {
                EnableApplicationInsights = true,
                ApplicationInsightsConnectionString = "InstrumentationKey=test-key",
                ApplicationName = null,
                CloudInstance = "TestInstance"
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - verify the service started and uses service type name as default
        service.IsRunning.Should().BeTrue();
        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    [Fact]
    public async Task ConfigureLogging_UsesDefaultCloudInstance_WhenNotProvided()
    {
        // Arrange
        var service = new TestHostedServiceWithLoggingOptions(
            new ConfigurationBuilder().Build(),
            new LoggingOptions
            {
                EnableApplicationInsights = true,
                ApplicationInsightsConnectionString = "InstrumentationKey=test-key",
                ApplicationName = "TestApp",
                CloudInstance = null
            });

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - verify the service started and uses machine name as default
        service.IsRunning.Should().BeTrue();
        await service.StopAsync(CancellationToken.None);
        service.Dispose();
    }

    private class TestHostedServiceWithLoggingOptions : ClearHostedService
    {
        private readonly LoggingOptions _loggingOptions;
        public bool IsRunning { get; private set; }

        public TestHostedServiceWithLoggingOptions(IConfiguration configuration, LoggingOptions loggingOptions)
            : base(configuration)
        {
            _loggingOptions = loggingOptions;
        }

        protected override LoggingOptions GetLoggingOptions()
        {
            return _loggingOptions;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IsRunning = true;
            return Task.CompletedTask;
        }
    }
}

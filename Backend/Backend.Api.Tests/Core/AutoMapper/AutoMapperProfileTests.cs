using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Backend.Api.Core;

namespace Backend.Api.Tests.Core.AutoMapper;

public class AutoMapperProfileTests
{
    // NOTE: OLD
    // [Fact]
    // public void AutoMapper_Configuration_IsValid()
    // {
    //     // Arrange: create a DI container for AutoMapper
    //     var services = new ServiceCollection();
    //
    //     services.AddAutoMapper(typeof(CourseProfile).Assembly);
    //
    //     using var serviceProvider = services.BuildServiceProvider();
    //
    //     var configuration = serviceProvider
    //         .GetRequiredService<IConfigurationProvider>();
    //
    //     // Act & Assert: throws if any mapping configuration is invalid
    //     configuration.AssertConfigurationIsValid();
    // }

    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(ApiAssemblyMarker).Assembly),
            NullLoggerFactory.Instance);

        // Act & Assert
        config.AssertConfigurationIsValid();
    }
}

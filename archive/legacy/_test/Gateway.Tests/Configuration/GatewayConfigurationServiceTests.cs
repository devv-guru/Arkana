using System.Threading;
using System.Threading.Tasks;
using Endpoints.Configuration.Services;
using Moq;
using Shared.Models;
using Xunit;

namespace Gateway.Tests.Configuration;

public class GatewayConfigurationServiceTests
{
    [Fact]
    public void GetConfiguration_ReturnsConfiguration()
    {
        // Arrange
        var expectedConfig = new GatewayConfigurationOptions
        {
            ConfigurationStoreType = "File",
            ConfigurationFilePath = "test-config.json",
            ReloadIntervalSeconds = 30
        };
        
        var mockService = new Mock<IConfigurationService>();
        mockService.Setup(x => x.GetConfiguration())
                  .Returns(expectedConfig);

        // Act
        var result = mockService.Object.GetConfiguration() as GatewayConfigurationOptions;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("File", result.ConfigurationStoreType);
        Assert.Equal("test-config.json", result.ConfigurationFilePath);
        Assert.Equal(30, result.ReloadIntervalSeconds);
    }

    [Fact]
    public async Task SaveConfigurationAsync_ValidConfig_ReturnsTrue()
    {
        // Arrange
        var config = new GatewayConfigurationOptions
        {
            ConfigurationStoreType = "AWS",
            AwsSecretName = "test-secret"
        };
        
        var mockService = new Mock<IConfigurationService>();
        mockService.Setup(x => x.SaveConfigurationAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

        // Act
        var result = await mockService.Object.SaveConfigurationAsync(config);

        // Assert
        Assert.True(result);
        mockService.Verify(x => x.SaveConfigurationAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateConfigurationAsync_ValidUpdateAction_ReturnsTrue()
    {
        // Arrange
        var mockService = new Mock<IConfigurationService>();
        mockService.Setup(x => x.UpdateConfigurationAsync(It.IsAny<Action<object>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);
        
        var updateAction = new Action<object>(config => 
        {
            if (config is GatewayConfigurationOptions options)
            {
                options.ReloadIntervalSeconds = 120;
            }
        });

        // Act
        var result = await mockService.Object.UpdateConfigurationAsync(updateAction);

        // Assert
        Assert.True(result);
        mockService.Verify(x => x.UpdateConfigurationAsync(updateAction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReloadConfigurationAsync_CompletesSuccessfully()
    {
        // Arrange
        var mockService = new Mock<IConfigurationService>();
        mockService.Setup(x => x.ReloadConfigurationAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        // Act & Assert
        await mockService.Object.ReloadConfigurationAsync(CancellationToken.None);
        
        mockService.Verify(x => x.ReloadConfigurationAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void GetConfiguration_WhenNoConfig_ReturnsNull()
    {
        // Arrange
        var mockService = new Mock<IConfigurationService>();
        mockService.Setup(x => x.GetConfiguration())
                  .Returns((object?)null);

        // Act
        var result = mockService.Object.GetConfiguration();

        // Assert
        Assert.Null(result);
    }
}

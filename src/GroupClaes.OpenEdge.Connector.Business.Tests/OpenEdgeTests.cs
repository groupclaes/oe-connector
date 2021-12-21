using GroupClaes.OpenEdge.Connector.Business.Raw;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace GroupClaes.OpenEdge.Connector.Business.Tests
{
  public class OpenEdgeTests
  {
    private readonly OpenEdge openEdge;

    private readonly Mock<ILogger<OpenEdge>> loggerMock;
    private readonly Mock<IProxyInterface> proxyMock;
    private readonly Mock<IProxyProvider> proxyProviderMock;
    private readonly Mock<IChecksumService> checksumMock;

    public OpenEdgeTests()
    {
      loggerMock = new Mock<ILogger<OpenEdge>>();
      proxyMock = new Mock<IProxyInterface>();
      proxyProviderMock = new Mock<IProxyProvider>();
      checksumMock = new Mock<IChecksumService>();

      proxyProviderMock.Setup(x => x.CreateProxyInstance())
          .Returns(proxyMock.Object);

      openEdge = new OpenEdge(loggerMock.Object, checksumMock.Object, proxyProviderMock.Object);
    }

    [Fact]
    public void GetFilteredParameters_ShouldReturnEmptyHashAndNoRedactedIfNoParametersAreSet()
    {
      // Arrange
      Parameter[] parametes = Array.Empty<Parameter>();
      ProcedureRequest request = new ProcedureRequest
      {
        Parameters = parametes
      };

      // Act
      var result = openEdge.GetFilteredParameters(request, out var displayeable, out string parameterHash);


      // Assert
      Assert.False(result);
      Assert.Equal(string.Empty, parameterHash);
      Assert.Empty(displayeable);
    }
  }
}

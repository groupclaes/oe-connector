using System;
using System.Text;
using Xunit;

namespace GroupClaes.OpenEdge.Connector.Business.Tests
{
  public class ChecksumServiceTests
  {
    private readonly ChecksumService checksumService;
    public ChecksumServiceTests()
    {
      checksumService = new ChecksumService();
    }

    [Theory]
    [InlineData("zxmQUjcyAA2c0rWMeGvpke8cUabxX4kwk8KPIlIw",
      "F04AF32B23E11325909A7F74F23925BAA033CD1C97329C0826FEECFBF8AD683C")]
    [InlineData("TJvVZtoU0FbQGLY9KR34RYFXv6ShrwkSubsH7MGK",
      "9077870EE69DD8BA15E5DC7E5E21E9C04A9120815078EF8CC70CE2EC9F48ABB1")]
    public void Generate_ShouldHashStringCorrectly(string value, string expectedHash)
    {
      // Act
      var resultHash = checksumService.Generate(value);

      // Assert
      Assert.Equal(expectedHash, resultHash);
    }

    [Fact]
    public void Generate_ShouldHashStringBuilderCorrectly()
    {
      // Arrange
      StringBuilder stringBuilder = new StringBuilder("asdjasdhuoi2e9u12498!&$*(");
      string expectedHash = "29D09A6BD8916C541D5DF2BD1628EBB8EFB10366A7ADC5E5B9D6E1F79C7A1827";

      // Act
      var resultHash = checksumService.Generate(stringBuilder);

      // Assert
      Assert.Equal(expectedHash, resultHash);
    }

  }
}

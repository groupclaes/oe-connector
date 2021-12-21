using GroupClaes.OpenEdge.Connector.Business.Tests.Models;
using GroupClaes.OpenEdge.Connector.Shared;
using GroupClaes.OpenEdge.Connector.Shared.Models;
using Moq;
using Progress.Open4GL.DynamicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GroupClaes.OpenEdge.Connector.Business.Tests
{
  public class OpenEdgeStaticTests
  {
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(100)]
    [InlineData("I'm the default")]
    public void ExtractValueFromJsonElement_ShouldReturnCorrectValue(object expectedResult)
    {
      // Arrange
      JsonElement element = JsonSerializer.SerializeToElement(expectedResult);


      // Act
      var result = OpenEdge.ExtractValueFromJsonElement(element);


      // Assert
      Assert.Equal(result, expectedResult);
    }

    [Fact]
    public void ExtractValueFromJsonElement_ShouldReturnCorrectArrayWithChildren()
    {
      // Arrange
      ParseParent[] parseParents = new ParseParent[]
      {
        new ParseParent
        {
          Object = new ParseObject
          {
            Property1 = "Test1",
            Property2 = "Test2",
            Property3 = 3
          },
          OtherProperty = new ParseObject[]
          {
            new ParseObject
            {
              Property1 = "Test1",
              Property2 = "Test2",
              Property3 = 3
            },
            new ParseObject
            {
              Property1 = "Test2",
              Property2 = "Test3",
              Property3 = 4
            }
          }
        }
      };
      JsonElement element = JsonSerializer.SerializeToElement(parseParents);


      // Act
      var result = OpenEdge.ExtractValueFromJsonElement(element);


      // Assert
      var resultDictionary = Assert.IsType<Dictionary<string, object>[]>(result);
      Assert.Single(resultDictionary);

      var obj = Assert.IsType<Dictionary<string, object>>(resultDictionary[0].First(x => x.Key == "Object"));
      Assert.Equal("Test1", obj.First(x => x.Key == "Property1").Value as string);
      Assert.Equal("Test2", obj.First(x => x.Key == "Property2").Value as string);
      Assert.Equal(3, obj.First(x => x.Key == "Property3").Value as int? ?? 0);
    }

    [Fact]
    public void GenerateErrorResponse_ShouldMakeCopyAndSetTitleAndDescription()
    {
      // Arrange
      ProcedureResponse procedureResponse = new ProcedureResponse
      {
        Status = 200,
        LastModified = DateTime.UtcNow,
        OriginTime = 400,
        Procedure = "YoinkySploinky.w",
        Result = "I'm a result, yeaaah!"
      };

      ProcedureResult procedureResult = new ProcedureResult
      {
        StatusCode = 500,
        Title = "I'm a title",
        Description = "Description"
      };

      // Act
      var result = OpenEdge.GenerateErrorResponse(procedureResponse, procedureResult);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(500, result.Status);
      Assert.True(result.LastModified.HasValue);
      Assert.Equal(DateTime.UtcNow, result.LastModified.Value, TimeSpan.FromMinutes(1));
      Assert.Equal(400, result.OriginTime);
      Assert.Equal("YoinkySploinky.w", result.Procedure);
      Assert.Equal("I'm a result, yeaaah!", result.Result);

      Assert.Equal("I'm a title", result.Title);
      Assert.Equal("Description", result.Description);
    }

    [Theory]
    [InlineData("AED4914740264FDF57756313077C01F0F5DE84EED5349E6312BE029F145D4476")]
    [InlineData("Wankawanka")]
    public void GetCacheKey_ShouldReturnWithParameterHashIfNotNullOrEmpty(string parameterHash)
    {
      // Arrange
      var expectedKey = "OpenEdge:Procedures:procedure:" + parameterHash;

      // Act
      var result = OpenEdge.GetCachedKey("procedure", parameterHash);

      // Assert
      Assert.Equal(expectedKey, result);
    }

    [Theory]
    [InlineData("procedure")]
    [InlineData("test123")]
    public void GetCacheKey_ShouldReturnCorrectKeyWithProcedure(string procedure)
    {
      // Arrange
      var expectedKey = "OpenEdge:Procedures:" + procedure;

      // Act
      var result = OpenEdge.GetCachedKey(procedure, null);

      // Assert
      Assert.Equal(expectedKey, result);
    }

    [Fact]
    public void GetJsonBytes_ShouldParseObjectCorrect()
    {
      // Arrange
      string jsonString = "{\"property1\":\"test\",\"property2\":\"test2\",\"property3\":69}";
      byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

      var objectToParse = new ParseObject
      {
        Property1 = "test",
        Property2 = "test2",
        Property3 = 69
      };

      // Act
      var result = OpenEdge.GetJsonBytes(objectToParse);


      // Assert
      Assert.Equal(jsonBytes, result);
    }

    [Fact]
    public void GetOutputParameters_ShouldReturnEmptyListIfRequestParamsIsEmpty()
    {
      // Arrange
      Parameter[] parameters = Array.Empty<Parameter>();


      // Act
      var result = OpenEdge.GetOutputParameters(parameters, null);


      // Assert
      Assert.Empty(result);
    }

    [Fact]
    public void GetOutputParameters_ShouldReturnSingleEntryWithoutKeyOnSingleValueWithoutLabel()
    {
      // Arrange
      Parameter[] parameters = new Parameter[]
      {
        new Parameter { Output = true, Position = 1, Type = ParameterType.Integer },
        new Parameter { Output = false, Position = 2, Type = ParameterType.Integer },
        new Parameter { Output = false, Position = 3, Type = ParameterType.Integer },
      };
      Mock<ParameterSet> parameterSetMock = new Mock<ParameterSet>(1);
      parameterSetMock.Setup(x => x.getOutputParameter(1))
        .Returns(100);

      // Act
      var result = OpenEdge.GetOutputParameters(parameters, parameterSetMock.Object);

      // Assert
      Assert.Single(result);
      var first = result.FirstOrDefault();
      Assert.Equal(string.Empty, first.Key);
      Assert.Equal(100, first.Value);

      parameterSetMock.Verify(x => x.getOutputParameter(1), Times.Once);

      parameterSetMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void GetOutputParameters_ShouldReturnOutputsWithCorrectLabelsAndFallbackLabels()
    {
      // Arrange
      Parameter[] parameters = new Parameter[]
      {
        new Parameter { Output = true, Position = 1, Type = ParameterType.Integer, Label = "one" },
        new Parameter { Output = true, Position = 2, Type = ParameterType.Integer, Label = "two" },
        new Parameter { Output = true, Position = 3, Type = ParameterType.Integer },
      };
      Mock<ParameterSet> parameterSetMock = new Mock<ParameterSet>(3);
      parameterSetMock.Setup(x => x.getOutputParameter(1))
        .Returns(100);
      parameterSetMock.Setup(x => x.getOutputParameter(2))
        .Returns(200);
      parameterSetMock.Setup(x => x.getOutputParameter(3))
        .Returns(300);


      // Act
      var result = OpenEdge.GetOutputParameters(parameters, parameterSetMock.Object);


      // Assert
      Assert.NotEmpty(result);
      Assert.Equal(3, result.Count);

      var kvPair = result.First(x => x.Key == "one");
      Assert.Equal(100, kvPair.Value);
      kvPair = result.First(x => x.Key == "two");
      Assert.Equal(200, kvPair.Value);
      kvPair = result.First(x => x.Key == "3");
      Assert.Equal(300, kvPair.Value);
    }

    [Fact]
    public void GetParameterSetType_ShouldSetDefaultInputParameterToString()
    {
      // Arrange
      Parameter parameter = new Parameter
      {
        Output = false,
        Type = ParameterType.Undefined
      };


      // Act
      var result = OpenEdge.GetParameterSetType(parameter);


      // Assert
      Assert.Equal(1, result);
    }

    [Fact]
    public void GetParameterSetType_ShouldSetDefaultOutputParameterToJSON()
    {
      // Arrange
      Parameter parameter = new Parameter
      {
        Output = true,
        Type = ParameterType.Undefined
      };


      // Act
      var result = OpenEdge.GetParameterSetType(parameter);


      // Assert
      Assert.Equal(11, result);
      Assert.Equal(ParameterType.JSON, parameter.Type);
    }

    [Theory]
    [InlineData(ParameterType.JSON, 11)]
    [InlineData(ParameterType.String, 1)]
    [InlineData(ParameterType.Date, 2)]
    [InlineData(ParameterType.Boolean, 3)]
    [InlineData(ParameterType.Byte, 8)]
    [InlineData(ParameterType.LongChar, 39)]
    [InlineData(ParameterType.Integer, 4)]
    [InlineData(ParameterType.Decimal, 5)]
    [InlineData(ParameterType.Long, 7)]
    [InlineData(ParameterType.Integer64, 41)]
    [InlineData(ParameterType.Handle, 10)]
    [InlineData(ParameterType.MemPointer, 11)]
    [InlineData(ParameterType.RowId, 13)]
    [InlineData(ParameterType.COMHandle, 14)]
    [InlineData(ParameterType.DataTable, 15)]
    [InlineData(ParameterType.DynDataTable, 17)]
    [InlineData(ParameterType.DataSet, 36)]
    [InlineData(ParameterType.DateTime, 34)]
    [InlineData(ParameterType.DateTimeTZ, 40)]
    public void GetParameterSetType_ShouldReturnCorrectType(ParameterType parameterType, int expectedResult)
    {
      // Arrange
      Parameter parameter = new Parameter
      {
        Type = parameterType
      };


      // Act
      var result = OpenEdge.GetParameterSetType(parameter);


      // Assert
      Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetParsedOutputs_ShouldReturnDictionaryIfMoreThanOneItems()
    {
      // Arrange
      Dictionary<string, object> dictionary = new Dictionary<string, object>
      {
        { "1", null },
        { "2", null }
      };

      // Act
      var result = OpenEdge.GetParsedOutputs(dictionary);

      // Assert
      Assert.Equal(dictionary, result);
    }

    [Fact]
    public void GetParsedOutputs_ShouldReturnDictionaryIfOneWithKey()
    {
      // Arrange
      Dictionary<string, object> dictionary = new Dictionary<string, object>
      {
        { "1", null }
      };


      // Act
      var result = OpenEdge.GetParsedOutputs(dictionary);


      // Assert
      Assert.Equal(dictionary, result);
    }

    [Fact]
    public void GetParsedOutputs_ShouldReturnValueIfOnlyOneEntryWithoutKey()
    {
      // Arrange
      Dictionary<string, object> dictionary = new Dictionary<string, object>
      {
        { string.Empty, "I'm a value!" }
      };

      // Act
      var result = OpenEdge.GetParsedOutputs(dictionary);

      // Assert
      Assert.Equal("I'm a value!", result);
    }

    [Fact]
    public void GetProcedureFromBytes_ShouldParseJsonCorrect()
    {
      // Arrange
      var jsonBytes = Encoding.UTF8.GetBytes("{\"proc\":\"wsv1GetProdInfo.p\",\"result\":100,\"lastMod\":null,\"origTime\":283}");

      // Act
      var result = OpenEdge.GetProcedureFromBytes(jsonBytes);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("wsv1GetProdInfo.p", result.Procedure);
      Assert.Equal(100, ((JsonElement)result.Result).GetInt32());
      Assert.Null(result.LastModified);
      Assert.Equal(283, result.OriginTime);
    }

    [Fact]
    public void GetProcedureResult_ShouldReturnNullIfLengthInvalid()
    {
      // Arrange
      var input = "000";

      // Act
      var result = OpenEdge.GetProcedureResult(input);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void GetProcedureResult_ShouldReturnNullIfStatusCodeInvalid()
    {
      // Arrange
      var input = "000::Valid title";

      // Act
      var result = OpenEdge.GetProcedureResult(input);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void GetProcedureResult_ShouldReturnValidResult()
    {
      // Arrange
      var input = "404::Valid title";

      // Act
      var result = OpenEdge.GetProcedureResult(input);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(404, result.StatusCode);
      Assert.Equal("Valid title", result.Title);
      Assert.Null(result.Description);
    }

    [Theory]
    [InlineData("500::Something went wrong::This is a valid description: yes",
      "Something went wrong", "This is a valid description: yes")]
    [InlineData("500::Something went wrong::This is a valid description:: yes",
      "Something went wrong", "This is a valid description:: yes")]
    public void GetProcedureResult_ShouldReturnValidResultWithDescription(string input,
      string title, string description)
    {
      // Act
      var result = OpenEdge.GetProcedureResult(input);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(500, result.StatusCode);
      Assert.Equal(title, result.Title);
      Assert.Equal(description, result.Description);
    }
  }
}

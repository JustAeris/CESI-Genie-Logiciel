using EasySave.Console;

namespace EasySave.Tests;

public class ArgParserTests
{
    [Fact]
    public void Parse_Range_ReturnsConsecutiveIndices()
    {
        var result = ArgParser.Parse("1-3");
        Assert.Equal([1, 2, 3], result);
    }

    [Fact]
    public void Parse_Range_SingleElement()
    {
        var result = ArgParser.Parse("2-2");
        Assert.Equal([2], result);
    }

    [Fact]
    public void Parse_List_ReturnsSelectedIndices()
    {
        var result = ArgParser.Parse("1;3");
        Assert.Equal([1, 3], result);
    }

    [Fact]
    public void Parse_Single_ReturnsSingleIndex()
    {
        var result = ArgParser.Parse("2");
        Assert.Equal([2], result);
    }

    [Fact]
    public void Parse_List_ThreeElements()
    {
        var result = ArgParser.Parse("1;2;5");
        Assert.Equal([1, 2, 5], result);
    }

    [Fact]
    public void Parse_Range_FullRange()
    {
        var result = ArgParser.Parse("1-5");
        Assert.Equal([1, 2, 3, 4, 5], result);
    }
}

using StrivoIngestPublish;
using Xunit;

namespace StrivoIngestPublish.Tests;

public class CsvMessageBuilderTests
{
    // --- ParseCsvLine tests ---

    [Fact]
    public void ParseCsvLine_SimpleFields_ReturnsValues()
    {
        var result = CsvMessageBuilder.ParseCsvLine("id,name,value");
        Assert.Equal(["id", "name", "value"], result);
    }

    [Fact]
    public void ParseCsvLine_FieldWithLeadingSpace_PreservesWhitespace()
    {
        // RFC 4180: whitespace is part of the field value and must not be trimmed
        var result = CsvMessageBuilder.ParseCsvLine("id, name ,value");
        Assert.Equal(["id", " name ", "value"], result);
    }

    [Fact]
    public void ParseCsvLine_QuotedFieldWithComma_TreatedAsSingleField()
    {
        var result = CsvMessageBuilder.ParseCsvLine("id,\"last, first\",value");
        Assert.Equal(["id", "last, first", "value"], result);
    }

    [Fact]
    public void ParseCsvLine_EscapedDoubleQuoteInField_PreservesQuote()
    {
        var result = CsvMessageBuilder.ParseCsvLine("id,\"say \"\"hello\"\"\",value");
        Assert.Equal(["id", "say \"hello\"", "value"], result);
    }

    [Fact]
    public void ParseCsvLine_EmptyField_ReturnsEmptyString()
    {
        var result = CsvMessageBuilder.ParseCsvLine("id,,value");
        Assert.Equal(["id", "", "value"], result);
    }

    [Fact]
    public void ParseCsvLine_SingleField_ReturnsSingleElement()
    {
        var result = CsvMessageBuilder.ParseCsvLine("only");
        Assert.Equal(["only"], result);
    }

    // --- EscapeJson tests ---

    [Fact]
    public void EscapeJson_NoSpecialChars_ReturnsSameValue()
    {
        Assert.Equal("hello", CsvMessageBuilder.EscapeJson("hello"));
    }

    [Fact]
    public void EscapeJson_DoubleQuote_EscapesQuote()
    {
        Assert.Equal("say \\\"hi\\\"", CsvMessageBuilder.EscapeJson("say \"hi\""));
    }

    [Fact]
    public void EscapeJson_Backslash_EscapesBackslash()
    {
        Assert.Equal("a\\\\b", CsvMessageBuilder.EscapeJson("a\\b"));
    }

    // --- BuildMessage tests ---

    [Fact]
    public void BuildMessage_BasicRow_ProducesValidJsonWithSourceField()
    {
        var message = CsvMessageBuilder.BuildMessage("id,name", "1,Alice", "data.csv");

        Assert.StartsWith("{", message);
        Assert.EndsWith("}", message);
        Assert.Contains("\"source\":\"data.csv\"", message);
        Assert.Contains("\"id\":\"1\"", message);
        Assert.Contains("\"name\":\"Alice\"", message);
    }

    [Fact]
    public void BuildMessage_FewerValuesThanHeaders_UsesEmptyStringForMissingValues()
    {
        var message = CsvMessageBuilder.BuildMessage("id,name,extra", "1,Alice", "data.csv");

        Assert.Contains("\"extra\":\"\"", message);
    }

    [Fact]
    public void BuildMessage_SourceBlobName_IsEscapedInJson()
    {
        var message = CsvMessageBuilder.BuildMessage("col", "val", "path/to/my \"file\".csv");

        Assert.Contains("\"source\":\"path/to/my \\\"file\\\".csv\"", message);
    }

    [Fact]
    public void BuildMessage_QuotedCsvFields_AreUnquotedInJson()
    {
        var message = CsvMessageBuilder.BuildMessage("city", "\"New York\"", "data.csv");

        Assert.Contains("\"city\":\"New York\"", message);
    }
}

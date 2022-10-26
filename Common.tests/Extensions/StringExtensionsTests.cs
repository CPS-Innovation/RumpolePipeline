using Common.Domain.Extensions;
using FluentAssertions;
using Xunit;

namespace Common.tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void EscapeUriDataStringRfc3986_IfNullOrEmptyString_ReturnsEmptyString(string valueIn)
    {
        var convertedItem = valueIn.EscapeUriDataStringRfc3986();

        convertedItem.Should().BeEmpty();
    }

    [Fact]
    public void EscapeUriDataStringRfc3986_UrlEncodesAString_AsExpected()
    {
        const string itemToTest = "A TEST DOC.pdf";
        const string itemToMatch = "A%20TEST%20DOC.pdf";
        var convertedItem = itemToTest.EscapeUriDataStringRfc3986();

        convertedItem.Should().Be(itemToMatch);
    }
    
    [Fact]
    public void EscapeUriDataStringRfc3986_DoesNotChange_ANoneConvertibleString_AsExpected()
    {
        const string itemToTest = "james-crane.pdf";
        const string itemToMatch = "james-crane.pdf";
        var convertedItem = itemToTest.EscapeUriDataStringRfc3986();

        convertedItem.Should().Be(itemToMatch);
    }
}

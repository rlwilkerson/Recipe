using Recipe.Web.Services;

namespace Recipe.Tests.Services;

public class SlugServiceTests
{
    private readonly SlugService _slugService = new();

    [Fact]
    public void GenerateSlug_ConvertsToLowercase()
    {
        var result = _slugService.GenerateSlug("My Cookbook");
        Assert.Equal("my-cookbook", result);
    }

    [Fact]
    public void GenerateSlug_ConvertSpacesToHyphens()
    {
        var result = _slugService.GenerateSlug("hello world test");
        Assert.Equal("hello-world-test", result);
    }

    [Fact]
    public void GenerateSlug_StripsSpecialCharacters()
    {
        var result = _slugService.GenerateSlug("Hello, World!");
        Assert.Equal("hello-world", result);
    }

    [Fact]
    public void GenerateSlug_CollapsesMultipleHyphens()
    {
        var result = _slugService.GenerateSlug("a--b---c");
        Assert.Equal("a-b-c", result);
    }

    [Fact]
    public void GenerateSlug_TrimsLeadingAndTrailingHyphens()
    {
        var result = _slugService.GenerateSlug("-hello-");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void GenerateSlug_HandlesEmptyString()
    {
        var result = _slugService.GenerateSlug("");
        Assert.Equal("", result);
    }

    [Fact]
    public void GenerateSlug_HandlesWhitespaceOnly()
    {
        var result = _slugService.GenerateSlug("   ");
        Assert.Equal("", result);
    }

    [Fact]
    public void GenerateSlug_HandlesComplexInput()
    {
        var result = _slugService.GenerateSlug("My Grandma's Best Recipe (2024)!");
        Assert.Equal("my-grandmas-best-recipe-2024", result);
    }

    [Fact]
    public void GenerateSlug_HandlesNumbersAndLetters()
    {
        var result = _slugService.GenerateSlug("Recipe 123 ABC");
        Assert.Equal("recipe-123-abc", result);
    }

    [Fact]
    public void GenerateSlug_HandlesUnicodeCharacters()
    {
        var result = _slugService.GenerateSlug("Café Olé");
        Assert.Equal("caf-ol", result);
    }
}

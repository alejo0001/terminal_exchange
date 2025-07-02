using System.Collections.Immutable;
using CrmAPI.Application.Common.TemplateToolkit;
using FluentAssertions;
using Xunit;

namespace Application.UnitTests.Common.TemplateToolkit;

[TestSubject(typeof(TemplateProcessor))]
public class TemplateProcessorTest
{
    [Fact]
    public void ReplacePlaceholders_ShouldReturnEmptyString_WhenTemplateIsNullOrEmpty()
    {
        // Arrange
        string? nullTemplate = null;
        var emptyTemplate = string.Empty;

        // Act
        var resultForNull = TemplateProcessor.ReplacePlaceholders(
            nullTemplate!,
            replacements: ImmutableDictionary<string, string?>.Empty);

        var resultForEmpty = TemplateProcessor.ReplacePlaceholders(
            emptyTemplate,
            replacements: ImmutableDictionary<string, string?>.Empty);

        // Assert
        resultForNull.Should().BeEmpty();
        resultForEmpty.Should().BeEmpty();
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReturnOriginalTemplate_WhenEmptyReplacementDataIsEmpty()
    {
        // Arrange
        const string template = "Hello, {{user}}! Have a great day!";
        var replacements1 = new Dictionary<string, string?>();
        var replacements2 = ImmutableDictionary<string, string?>.Empty;

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, true, replacements1, replacements2);

        // Assert
        result.Should().Be("Hello, {{user}}! Have a great day!");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReplaceAllOccurrences_WhenUsingStringReplacements()
    {
        // Arrange
        const string template = "Hello, {{name}}! Welcome to {{company}}. {{special!}}, {{spe-cial}}.";
        var replacements = new Dictionary<string, string?>
        {
            { "{{name}}", "John" },
            { "{{company}}", "OpenAI" },
            { "{{special!}}", "$$" },
            { "{{spe-cial}}", "_" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello, John! Welcome to OpenAI. $$, _.");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReplaceAllOccurrences_WhenUsingDelegateReplacements()
    {
        // Arrange
        const string template = "Good morning, {{user}}! You have {{count}} new messages.";
        var replacements = new Dictionary<string, Func<string?>?>
        {
            { "{{user}}", () => "Alice" },
            { "{{count}}", () => "5" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Good morning, Alice! You have 5 new messages.");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldHandleDuplicateKeys_LastValueShouldWin()
    {
        // Arrange
        const string template = "Name: {{name}}, Greeting: {{greeting}}";
        var replacements1 = new Dictionary<string, string?>
        {
            { "{{name}}", "FirstName" },
            { "{{greeting}}", "Hello" },
        };
        var replacements2 = new Dictionary<string, string?>
        {
            { "{{name}}", "LastName" }, // This should overwrite the previous value
            { "{{greeting}}", "Hi" }, // This should also overwrite the previous value
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, true, replacements1, replacements2);

        // Assert
        result.Should().Be("Name: LastName, Greeting: Hi");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldIgnoreWhitespaceInReplacementValues()
    {
        // Arrange
        const string template = "Greetings, {{title}} {{lastName}}!";
        var replacements = new Dictionary<string, string?>
        {
            { "{{title}}", "   Mr.   " },
            { "{{lastName}}", "  Doe  " },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Greetings, Mr. Doe!");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReturnOriginalTemplate_WhenNoReplacementsAreGiven()
    {
        // Arrange
        const string template = "No placeholders here.";

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(
            template,
            replacements: ImmutableDictionary<string, Func<string?>?>.Empty);

        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public void ReplacePlaceholders_ShouldNotReplaceUnmatchedPlaceholders()
    {
        // Arrange
        const string template = "Hello, {{user}}! Have a great day!";
        var replacements = new Dictionary<string, string?>
        {
            { "{{notUser}}", "Someone" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello, {{user}}! Have a great day!");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldHandleReplacementWithNullValue_ReturnEmptyReplacement()
    {
        // Arrange
        const string template = "Hello, {{user}}!";
        var replacements = new Dictionary<string, string?>
        {
            { "{{user}}", null },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello, !");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldRespectCaseSensitivity_WhenCaseSensitivityIsEnabled()
    {
        // Arrange
        const string template = "Case test: {{key1}}, {{key2}}, {{KEY3}}, {{KEY4}}.";
        var replacements = new Dictionary<string, string?>
        {
            { "{{KEY1}}", "Upper" },
            { "{{key2}}", "Lower" },
            { "{{KEY3}}", "Upper" },
            { "{{key4}}", "Lower" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, false, replacements);

        // Assert
        result.Should().Be("Case test: {{key1}}, Lower, Upper, {{KEY4}}.");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldRespectCaseInsensitivity_WhenCaseSensitivityIsDisabled()
    {
        // Arrange
        const string template = "Case test: {{key1}}, {{key2}}, {{KEY3}}, {{KEY4}}.";
        var replacements = new Dictionary<string, string?>
        {
            { "{{Key1}}", "Upper" },
            { "{{key2}}", "Lower" },
            { "{{Key3}}", "Upper" },
            { "{{key4}}", "Lower" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, true, replacements);

        // Assert
        result.Should().Be("Case test: Upper, Lower, Upper, Lower.");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReplacePlaceholderAtBeginningAndEnd()
    {
        // Arrange
        const string template = "{{greeting}} everyone, have a great day {{closing}}";
        var replacements = new Dictionary<string, string?>
        {
            { "{{greeting}}", "Hello" },
            { "{{closing}}", "Goodbye" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello everyone, have a great day Goodbye");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldReplaceOverlappingPlaceholders_Correctly()
    {
        // Arrange
        const string template = "Hello {{user}}, your ID is {{userId}}.";
        var replacements = new Dictionary<string, string?>
        {
            { "{{user}}", "Alice" },
            { "{{userId}}", "12345" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello Alice, your ID is 12345.");
    }

    [Fact]
    public void ReplacePlaceholders_ShouldNotReplaceEmptyPlaceholder()
    {
        // Arrange
        const string template = "Hello, {{}}! Have a great day!";
        var replacements = new Dictionary<string, string?>
        {
            { "{{user}}", "Alice" },
        };

        // Act
        var result = TemplateProcessor.ReplacePlaceholders(template, replacements: replacements);

        // Assert
        result.Should().Be("Hello, {{}}! Have a great day!");
    }
}

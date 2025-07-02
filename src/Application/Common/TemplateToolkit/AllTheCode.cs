using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CrmAPI.Application.Common.Utils;
using IntranetMigrator.Domain.Entities;
using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.TemplateToolkit;

public enum VariableType
{
    Static,
    Translatable,
    Contextual,
}

public class TemplateVariableNameComparer : IEqualityComparer<TemplateVariable>
{
    public bool Equals(TemplateVariable? x, TemplateVariable? y) => x is { } && y is { } && x.Name.Equals(y.Name);

    public int GetHashCode(TemplateVariable obj) => obj.Name.GetHashCode();
}

public record TemplateVariable(
    string Name,
    VariableType Type,
    Func<string>? StaticValueProvider = null,
    string? TranslationKey = null,
    string? Description = null
);

public static class TemplateVariableRegistry
{
    public static readonly IReadOnlySet<TemplateVariable> Variables
        = new HashSet<TemplateVariable>(new TemplateVariableNameComparer())
        {
            new(
                TemplateVariableNameRegistry.TechUniversity,
                VariableType.Static,
                () => GeneralVariableHelper.GetStaticValue(nameof(TemplateVariableNameRegistry.TechUniversity))),
            new(
                TemplateVariableNameRegistry.Greetings,
                VariableType.Translatable),
            new(
                TemplateVariableNameRegistry.CourseType0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseTitle0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.UserSignature,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseGuarantor0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseScholarship0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseDiscountPercentage0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseRestDiscountPercentage0,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.InitialScholarshipValidityDate,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CommercialFullName,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CommercialPhone,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CommercialEmail,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.SupervisorPhone,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.SupervisorFullName,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.SupervisorEmail,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.ContactFirstName,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.ContactFullName,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourtesyTitleSupervisor,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CommercialCourtesyTitle,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.ContactCourtesyTitle,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.CourseStartDateN,
                VariableType.Contextual),
            new(
                TemplateVariableNameRegistry.AcademicConsultant,
                VariableType.Contextual,
                Description: "Translated using gender"),
        };

    public static TemplateVariable? GetVariable(string variableName) =>
        Variables.FirstOrDefault(v => v.Name == variableName);
}

/// <remarks>
///     ContextualVariableResolver placeholder for future implementation
/// </remarks>
public class ContextualVariableResolver : IVariableResolver
{
    public Func<string> Resolve(TemplateVariable variable, Template template) =>
        // throw new NotImplementedException("Contextual value resolution should be handled by the specific use case.");
        () => string.Empty;
}

public class ReplaceDataResolver
{
    private readonly Template _template;
    private readonly IVariableResolver _staticResolver;
    private readonly IVariableResolver _translatableResolver;
    private readonly IVariableResolver _contextualResolver;

    public ReplaceDataResolver(Template template)
    {
        _template = template;
        _staticResolver = new StaticVariableResolver();
        _translatableResolver = new TranslatableVariableResolver();
        _contextualResolver = new ContextualVariableResolver();
    }

    public Dictionary<string, Func<string>> GenerateBodyReplacementData() => GenerateReplacementData(_template.Body);

    public Dictionary<string, Func<string>> GenerateSubjectReplacementData() =>
        GenerateReplacementData(_template.Subject);

    private Dictionary<string, Func<string>> GenerateReplacementData(string templateContent)
    {
        var replacementData = new Dictionary<string, Func<string>>();


        var placeholders = TemplateParser.Parse(templateContent);
        foreach (var placeholder in placeholders)
        {
            // placeholder is  $PLACEHOLDER$
            var templateVariable = TemplateVariableRegistry.GetVariable(placeholder);

            if (templateVariable is not { })
            {
                continue;
            }

            var resolver = GetResolverForType(templateVariable.Type);

            replacementData[$"${placeholder}$"] = resolver.Resolve(templateVariable, _template);
        }

        return replacementData;
    }

    private IVariableResolver GetResolverForType(VariableType type) =>
        type switch
        {
            VariableType.Static => _staticResolver,
            VariableType.Translatable => _translatableResolver,
            _ => _contextualResolver,
        };
}

public static class GeneralVariableHelper
{
    public static string GetStaticValue(string key) =>
        key switch
        {
            nameof(TemplateVariableNameRegistry.TechUniversity) => "TECH",

            _ => "[Undefined Value]",
        };
}

public static partial class TemplateParser
{
    public static List<string> Parse(string templateContent)
    {
        // Logic to extract placeholders from the given template content.
        // Example: find all {{variable}} patterns and return them.
        return TemplatePlaceholderRegex()
            .Matches(templateContent)
            .Select(match => match.Groups[1].Value)
            .ToList();
    }

    [GeneratedRegex(@"\$(\w+)\$")]
    private static partial Regex TemplatePlaceholderRegex();
}

public static class TranslationApi
{
    private const string DefaultLanguageES = "ES";

    public static string GetTranslationAsync(
        string translationKey,
        string? languageName = DefaultLanguageES,
        EmployeeGender? gender = EmployeeGender.Male)
    {
        // Simulate an async API call to get the translated value.
        // await Task.Delay(100); // Simulate latency
        return translationKey switch
        {
            nameof(TemplateVariableNameRegistry.AcademicConsultant) => TranslationsHelper.GetAcademicConsultantRol(
                languageName ?? DefaultLanguageES,
                gender),

            _ => throw new ArgumentOutOfRangeException(nameof(translationKey), translationKey, null),
        };
    }
}

public interface IVariableResolver
{
    Func<string> Resolve(TemplateVariable variable, Template template);
}

public class StaticVariableResolver : IVariableResolver
{
    public Func<string> Resolve(TemplateVariable variable, Template template) => variable.StaticValueProvider!;
}

public class TranslatableVariableResolver : IVariableResolver
{
    public Func<string> Resolve(TemplateVariable variable, Template template)
    {
        return () => TranslationApi.GetTranslationAsync(variable.TranslationKey!, template.Language?.Name);
    }
}

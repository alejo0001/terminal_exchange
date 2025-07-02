using System;
using System.Collections.Generic;
using CrmAPI.Application.Common.BusinessAlgorithms;
using CrmAPI.Application.Common.Dtos;
using CrmAPI.Application.Common.TemplateToolkit;
using CrmAPI.Contracts.Dtos;
using IntranetMigrator.Domain.Entities;
using static CrmAPI.Application.Common.Utils.TemplateVariableNameRegistry;

namespace CrmAPI.Application.Common.Utils;

public static class EmailContentHelper
{
    public const string TechInstitutionName = "Tech";

    /// <summary>
    ///     TODO: Explain what it does. What does magic number 7 mean?
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="languageId"></param>
    /// <returns>TODO: What is the meaning, or what is the result?</returns>
    public static string FormatEmailSubjectRtl(string subject, int? languageId) =>
        languageId switch
        {
            7 => $"\u202B{subject}\u202C",
            _ => subject,
        };

    public static string ReplaceBodyTemplateGeneralVariables(string body, Process process) =>
        body
            .Replace("$CONTACT_FIRST_NAME$", process.Contact.Name)
            .Replace("$GREETINGS$", "Estimado/a")
            .Replace("$CONTACT_FULL_NAME$", $"{process.Contact.Name}{process.Contact.FirstSurName}")
            .Replace("$COURTESY_TITLE_SUPERVISOR$", "")
            .Replace("$COURTESY_TITLE_SUPERVISOR$", "")
            .Replace("$COMMERCIAL_COURTESY_TITLE$", "")
            .Replace("$CONTACT_COURTESY_TITLE$", "")
            .Replace("$USER_SIGNATURE$", TechInstitutionName.ToUpperInvariant())
            .Replace("$TECH_UNIVERSITY$", TechInstitutionName.ToUpperInvariant());

    public static string ReplaceRecords2BodyTemplate(
        Template template,
        Employee? employee,
        ManagerDto? manager,
        Process process)
    {
        var academicConsultant = TranslationsHelper.GetAcademicConsultantRol(template.Language.Name, employee?.Gender);

        var commercialFullName = $"{employee?.User?.Name ?? string.Empty} {employee?.User?.Surname ?? string.Empty}";

        var commercialPhone = $"{employee?.CorporatePhonePrefix ?? string.Empty}{
            employee?.CorporatePhone ?? string.Empty}";

        var supervisorFullName = $"{manager?.givenName ?? string.Empty} {manager?.surname ?? string.Empty}";

        var replacements = GetRecords2Replacements(
            () => academicConsultant,
            () => commercialFullName,
            () => commercialPhone,
            () => employee?.CorporateEmail,
            () => manager?.corporatePhone,
            () => supervisorFullName,
            () => manager?.corporateEmail);

        var genericReplacements = GetGenericReplacements(process);

        return TemplateProcessor.ReplacePlaceholders(template.Body, true, replacements, genericReplacements);
    }

    public static string ReplaceScholarshipActivationSecondCourseTemplate(
        string body,
        ContactLead? contactLead,
        CourseImportedTlmkDto? courseImportedTlmk,
        Process process,
        string convocationDate,
        Func<ContactLead?, CourseImportedTlmkDto?, ProductPricingAlgorithms.DiscountPercent> getDiscountPercentDlgt)
    {
        var (discount, restDiscountPercentage) = getDiscountPercentDlgt(contactLead, courseImportedTlmk);

        var courseScholarship = GetCourseScholarship(
            courseImportedTlmk?.Country.Code,
            contactLead?.AreaUrl,
            process.Id);

        var replacements = GetScholarshipReplacements2Courses(
            () => courseImportedTlmk?.CourseType,
            () => courseImportedTlmk?.Title,
            () => courseImportedTlmk?.GuarantorName,
            () => convocationDate,
            () => courseScholarship,
            () => $"{discount:N0}",
            () => $"{restDiscountPercentage:N0}");

        var genericReplacements = GetGenericReplacements(process);

        return TemplateProcessor.ReplacePlaceholders(body, true, replacements, genericReplacements);
    }

    public static string GetScholarshipActivationBodyTemplate(
        Template template,
        IPricesForTlmkParamsDto rawData,
        ContactLead? contactLead,
        CourseImportedTlmkDto? courseImportedTlmk,
        Process process,
        string convocationDate,
        Func<decimal?, decimal> sanitizeDiscountPercentDlgt)
    {
        var (discount, restDiscountPercentage) = ProductPricingAlgorithms
            .GetScholarshipDiscountPercent(contactLead, courseImportedTlmk, sanitizeDiscountPercentDlgt);

        var courseScholarship = GetCourseScholarship(courseImportedTlmk?.Country.Code, rawData.AreaUrl, process.Id);

        var replacements = GetScholarshipReplacements(
            () => courseImportedTlmk?.CourseType,
            () => courseImportedTlmk?.Title,
            () => courseImportedTlmk?.GuarantorName,
            () => convocationDate,
            () => courseScholarship,
            () => discount.ToString("N0"),
            () => restDiscountPercentage.ToString("N0"));

        replacements.Add($"${CourseStartDateN}$", convocationDate);

        var genericReplacements = GetGenericReplacements(process);

        return TemplateProcessor.ReplacePlaceholders(template.Body, true, replacements, genericReplacements);
    }

    public static string ReplaceGeneralScholarshipActivation2BodyTemplate(
        string body,
        ContactLead? contactLead,
        CourseImportedTlmkDto? courseImportedTlmk,
        Process process,
        string convocationDate,
        Func<ContactLead?, CourseImportedTlmkDto?, ProductPricingAlgorithms.DiscountPercent> getDiscountPercentDlgt)
    {
        var (discount, restDiscountPercentage) = getDiscountPercentDlgt(contactLead, courseImportedTlmk);

        var courseScholarship = GetCourseScholarship(
            courseImportedTlmk?.Country.Code,
            contactLead?.AreaUrl,
            process.Id);

        var replacements = GetScholarshipReplacements(
            () => courseImportedTlmk?.CourseType,
            () => courseImportedTlmk?.Title,
            () => courseImportedTlmk?.GuarantorName,
            () => convocationDate,
            () => courseScholarship,
            () => $"{discount:N0}",
            () => $"{restDiscountPercentage:N0}");

        var genericReplacements = GetGenericReplacements(process);

        return TemplateProcessor.ReplacePlaceholders(body, true, replacements, genericReplacements);
    }

    public static string GenerateSubjectFromTemplate(string subject, int? languageId, Func<string?>? convocationDateDlgt)
    {
        var finalTemplate = FormatEmailSubjectRtl(subject, languageId);

        var replacementDelegates = GetSubjectReplacements(
            TechInstitutionName.ToUpperInvariant,
            convocationDateDlgt,
            TechInstitutionName.ToUpperInvariant);

        return TemplateProcessor.ReplacePlaceholders(finalTemplate, replacements: replacementDelegates);
    }

    /// <summary>
    ///     TODO: Needs explanation of its business or technical purpose.
    /// </summary>
    /// <returns>TODO: What is it, what it returns?</returns>
    public static string GetCourseScholarship(string? courseCountryCode, string? facultyName, int processId)
    {
        if (string.IsNullOrWhiteSpace(courseCountryCode) || string.IsNullOrWhiteSpace(facultyName))
        {
            return string.Empty;
        }

        var fac = facultyName[..3].ToUpperInvariant();

        return $"{courseCountryCode}-{fac}-{processId}";
    }

    public static Dictionary<string, string?> GetGenericReplacements(Process process) =>
        GetGenericReplacements(
            () => process.Contact.Name,
            () => "Estimado/a",
            () => $"{process.Contact.Name}{process.Contact.FirstSurName}",
            userSignatureDlgt: TechInstitutionName.ToUpperInvariant,
            institutionDlgt: TechInstitutionName.ToUpperInvariant);

    public static Dictionary<string, string?> GetGenericReplacements(
        Func<string?>? contactFirstNameDlgt = null,
        Func<string?>? greetingDlgt = null,
        Func<string?>? fullNameDlgt = null,
        Func<string?>? courtesyTitleSuperDlgt = null,
        Func<string?>? courtesyTitleCommercialDlgt = null,
        Func<string?>? courtesyTitleContactDlgt = null,
        Func<string?>? userSignatureDlgt = null,
        Func<string?>? institutionDlgt = null) =>
        new()
        {
            { "$CONTACT_FIRST_NAME$", contactFirstNameDlgt?.Invoke() },
            { "$GREETINGS$", greetingDlgt?.Invoke() },
            { "$CONTACT_FULL_NAME$", fullNameDlgt?.Invoke() },
            { "$COURTESY_TITLE_SUPERVISOR$", courtesyTitleSuperDlgt?.Invoke() },
            { "$COMMERCIAL_COURTESY_TITLE$", courtesyTitleCommercialDlgt?.Invoke() },
            { "$CONTACT_COURTESY_TITLE$", courtesyTitleContactDlgt?.Invoke() },
            { "$USER_SIGNATURE$", userSignatureDlgt?.Invoke() },
            { "$TECH_UNIVERSITY$", institutionDlgt?.Invoke() },
        };

    public static Dictionary<string, string?> GetRecords2Replacements(
        Func<string?>? academicConsultantDlgt,
        Func<string?>? commercialFullNameDlgt,
        Func<string?>? commercialPhoneDlgt,
        Func<string?>? commercialEmailDlgt,
        Func<string?>? supervisorPhoneDlgt,
        Func<string?>? supervisorFullNameDlgt,
        Func<string?>? supervisorEmailDlgt) =>
        new()
        {
            { "$ACADEMIC_CONSULTANT$", academicConsultantDlgt?.Invoke() },
            { "$COMMERCIAL_FULL_NAME$", commercialFullNameDlgt?.Invoke() },
            { "$COMMERCIAL_PHONE$", commercialPhoneDlgt?.Invoke() },
            { "$COMMERCIAL_EMAIL$", commercialEmailDlgt?.Invoke() },
            { "$SUPERVISOR_PHONE$", supervisorPhoneDlgt?.Invoke() },
            { "$SUPERVISOR_FULL_NAME$", supervisorFullNameDlgt?.Invoke() },
            { "$SUPERVISOR_EMAIL$", supervisorEmailDlgt?.Invoke() },
        };

    public static Dictionary<string, string?> GetScholarshipReplacements(
        Func<string?>? courseTypeDlgt = null,
        Func<string?>? courseTitleDlgt = null,
        Func<string?>? courseGuarantorDlgt = null,
        Func<string?>? convocationDateDlgt = null,
        Func<string?>? courseScholarshipDlgt = null,
        Func<string?>? courseDiscountPercentDlgt = null,
        Func<string?>? courseRestDiscountPercentDlgt = null) =>
        new()
        {
            { "$INITIAL_SCHOLARSHIP_VALIDITY_DATE$", convocationDateDlgt?.Invoke() },
            { "$COURSE_TYPE_0$", courseTypeDlgt?.Invoke() },
            { "$COURSE_TITLE_0$", courseTitleDlgt?.Invoke() },
            { "$COURSE_GUARANTOR_0$", courseGuarantorDlgt?.Invoke() },
            { "$COURSE_SCHOLARSHIP_0$", courseScholarshipDlgt?.Invoke() },
            { "$COURSE_DISCOUNT_PERCENTAGE_0$", courseDiscountPercentDlgt?.Invoke() },
            { "$COURSE_REST_DISCOUNT_PERCENTAGE_0$", courseRestDiscountPercentDlgt?.Invoke() },
        };

    public static Dictionary<string, string?> GetScholarshipReplacements2Courses(
        Func<string?>? courseTypeDlgt = null,
        Func<string?>? courseTitleDlgt = null,
        Func<string?>? courseGuarantorDlgt = null,
        Func<string?>? convocationDateDlgt = null,
        Func<string?>? courseScholarshipDlgt = null,
        Func<string?>? courseDiscountPercentDlgt = null,
        Func<string?>? courseRestDiscountPercentDlgt = null) =>
        new()
        {
            { "$INITIAL_SCHOLARSHIP_VALIDITY_DATE$", convocationDateDlgt?.Invoke() },
            { "$COURSE_TYPE_1$", courseTypeDlgt?.Invoke() },
            { "$COURSE_TITLE_1$", courseTitleDlgt?.Invoke() },
            { "$COURSE_GUARANTOR_1$", courseGuarantorDlgt?.Invoke() },
            { "$COURSE_SCHOLARSHIP_1$", courseScholarshipDlgt?.Invoke() },
            { "$COURSE_DISCOUNT_PERCENTAGE_1$", courseDiscountPercentDlgt?.Invoke() },
            { "$COURSE_REST_DISCOUNT_PERCENTAGE_1$", courseRestDiscountPercentDlgt?.Invoke() },
        };

    public static Dictionary<string, Func<string?>?> GetSubjectReplacements(
        Func<string?>? techInstitutionName,
        Func<string?>? initialScholarshipValidityDateDlgt,
        Func<string?>? userSignature) =>
        new()
        {
            { "$TECH_UNIVERSITY$", techInstitutionName },
            { "$INITIAL_SCHOLARSHIP_VALIDITY_DATE$", initialScholarshipValidityDateDlgt },
            { "$USER_SIGNATURE$", userSignature },
        };
}

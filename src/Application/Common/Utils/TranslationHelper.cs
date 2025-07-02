using IntranetMigrator.Domain.Enums;

namespace CrmAPI.Application.Common.Utils;

public static class TranslationsHelper
{
    public static string GetAcademicConsultantRol(string languageName, EmployeeGender? gender) =>
        languageName.ToUpperInvariant() switch
        {
            "FR" => gender == EmployeeGender.Male
                ? "conseiller commercial"
                : "conseillère commerciale",

            "DE" => gender == EmployeeGender.Male
                ? "Berater Kommerziell"
                : "Kommerzieller Berater",

            "IT" => "consulente commerciale",

            "EN" => "commercial advisor",

            _ => gender == EmployeeGender.Male
                ? "asesor comercial"
                : "asesora comercial",
        };
}

using IntranetMigrator.Domain.Entities;

namespace CrmAPI.Application;

/// <summary>
///     As of creation, these Codes are present within <see cref="Template.Name" />, they don't tend to be complete
///     values of the properties of <see cref="Template" /> entity.
/// </summary>
/// <remarks>
///     <para>
///         Constant values use parentheses purposely, because different "markers" can have overlapping fragments and
///         "Contains" lookup can return false-positive results, effectively wrong Templates. Hence, parentheses promote
///         stricter pattern-like lookup, but they still can be removed easily if some wildcard-like behavior is desirable.
///     </para>
///     <para>
///         TODO: Usage soon to be changed to work with new field "Code" in <see cref="Template" /> entity.
///     </para>
/// </remarks>
public static class TemplateCodeRegistry
{
    /// <summary>
    ///     Synonym(s): <i>BECA</i>.
    /// </summary>
    public const string ScholarshipActivation = "(00.A)";

    /// <summary>
    ///     Synonym(s): <i>BECA</i>.
    /// </summary>
    public const string ScholarshipActivationFixedPercentages = "(000.A)";

    /// <summary>
    ///     Synonym(s): <i>BECA</i>, "Registry 2" wave.
    /// </summary>
    public const string ScholarshipActivationR2 = "(R00.A)";

    public const string Records2 = "(R0.A)";

    public const string NoInterestedCourseR2 = "(R00.B)";

    public const string CloseProcesses = "(1.2.A)";

    public const string ScholarshipActivationR2Courses = "(R00.AA)";

    /// <summary>
    ///     "Mail de Activación de Beca ACTIVACIONES CON PROGRAMA ELEGIDO". Synonym(s): BECA, ACTIVACIÓN.
    /// </summary>
    public const string ScholarshipForActivationWith2Courses = "(A00.A)";
}

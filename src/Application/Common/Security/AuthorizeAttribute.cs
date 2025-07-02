using System;

namespace CrmAPI.Application.Common.Security;

/// <summary>
///     Specifies that the type this attribute is applied to requires authorization.
/// </summary>
/// <remarks>
///     This is mere marker with optional Roles information. Authorization procedure must be done some other component.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public class AuthorizeAttribute : Attribute
{
    public string? Roles { get; set; }
}

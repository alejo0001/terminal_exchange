using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CrmAPI.Infrastructure.Utilities;

public static class CustomEnvironments
{
    public const string NSwagRunning = "NSWAG_RUNNING";
}

public static class EnvironmentHelper
{
    /// <summary>
    ///     It detects whether <c>NSWAG_RUNNING=TRUE</c> has been set, by any means of ASP.NET configuration source.
    /// </summary>
    /// <returns></returns>
    public static bool IsNSwagRunning(this IConfiguration configuration) =>
        configuration[CustomEnvironments.NSwagRunning]
            ?.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
        ?? false;

    /// <summary>
    ///     Check whether Environment is either <see cref="Environments.Production" /> or <see cref="Environments.Staging" />
    /// </summary>
    /// <param name="hostEnvironment"></param>
    public static bool IsLikeProduction(this IHostEnvironment hostEnvironment) =>
        hostEnvironment.IsProduction() || hostEnvironment.IsStaging();
}

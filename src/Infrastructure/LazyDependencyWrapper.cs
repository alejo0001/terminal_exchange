using System;
using Microsoft.Extensions.DependencyInjection;

namespace CrmAPI.Infrastructure;

/// <summary>
///     Allows to postpone creation of potentially costly dependency. It can be registered as
///     <see cref="ServiceLifetime.Transient" />,cause the dependency type must be registered always separately with
///     desired lifetime.
/// </summary>
/// <remarks>Generic purpose type, register it as open generic service.</remarks>
/// <typeparam name="TService">Service type, dependency to be retrieved.</typeparam>
public class LazyDependencyWrapper<TService> : Lazy<TService>
    where TService : class
{
    public LazyDependencyWrapper(IServiceProvider serviceProvider)
        : base(serviceProvider.GetRequiredService<TService>) { }
}

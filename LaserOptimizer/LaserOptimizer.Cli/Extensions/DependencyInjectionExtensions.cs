using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace DroppedCode.LaserOptimizer.Cli.Extensions;
public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddSingletonWithImplementation<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(this IServiceCollection services)
              where TService : class
              where TImplementation : class, TService
  {
    services.AddSingleton<TImplementation>();
    return services.AddSingleton(typeof(TService), (s) => s.GetRequiredService<TImplementation>());
  }
}

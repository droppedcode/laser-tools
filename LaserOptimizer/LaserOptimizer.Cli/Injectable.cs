using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Binding;

namespace DroppedCode.LaserOptimizer.Cli;

internal class Injectable<T> : BinderBase<T>
{
  protected override T GetBoundValue(BindingContext bindingContext)
  {
    return (T)bindingContext.GetRequiredService(typeof(T));
  }
}

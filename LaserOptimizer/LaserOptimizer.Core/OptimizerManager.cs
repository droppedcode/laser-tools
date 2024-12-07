using DroppedCode.LaserOptimizer.Core.Optimizers;
using Svg;

namespace DroppedCode.LaserOptimizer.Core;

public class OptimizerManager
{
  private IOptimizer[] _optimizers;

  public OptimizerManager(IEnumerable<IOptimizer> optimizers) => _optimizers = optimizers.OrderBy(o => o.Order).ToArray();

  public async Task<SvgDocument> OptimizeAsync(SvgDocument document)
  {
    foreach (var optimizer in _optimizers)
    {
      document = await optimizer.OptimizeAsync(document);
    }

    return document;
  }
}

using Svg;

namespace DroppedCode.LaserOptimizer.Core.Optimizers;

public interface IOptimizer
{
  /// <summary>
  /// Order in the optimizer list.
  /// </summary>
  int Order { get; }

  /// <summary>
  /// Optimize the svg.
  /// </summary>
  Task<SvgDocument> OptimizeAsync(SvgDocument document);
}

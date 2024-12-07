using Svg;

namespace DroppedCode.LaserOptimizer.Core.PreProcessors;
public interface IPreProcessor
{
  /// <summary>
  /// Order in the optimizer list.
  /// </summary>
  int Order { get; }

  /// <summary>
  /// Post process the svg.
  /// </summary>
  IAsyncEnumerable<(string name, SvgDocument document)> ProcessAsync(string name, SvgDocument document);
}

using Svg;

namespace DroppedCode.LaserOptimizer.Core.Optimizers;
public class RemoveEmptyGroup : IOptimizer
{
  public int Order => 200;

  public Task<SvgDocument> OptimizeAsync(SvgDocument document)
  {
    RemoveEmptyGroupFromElement(document);

    return Task.FromResult(document);
  }

  private static void RemoveEmptyGroupFromElement(SvgElement element)
  {
    for (var i = element.Children.Count - 1; i >= 0; i--)
    {
      var child = element.Children[i];
      if (child is SvgGroup group && group.Children.Count == 0)
      {
        element.Children.RemoveAt(i);
      }
      else
      {
        RemoveEmptyGroupFromElement(child);
      }
    }
  }
}

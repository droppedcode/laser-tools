using DroppedCode.LaserOptimizer.Core.Optimizers;
using Svg;
using System.Drawing;

namespace DroppedCode.LaserOptimizer.Core.PreProcessors;

public class ColorSplitter : IPreProcessor
{
  private readonly RemoveEmptyGroup _removeEmptyGroups;

  public int Order => 1000;

  public ColorSplitter(RemoveEmptyGroup removeEmptyGroups)
  {
    _removeEmptyGroups = removeEmptyGroups;
  }

  public async IAsyncEnumerable<(string name, SvgDocument document)> ProcessAsync(string name, SvgDocument document)
  {
    var colors = new HashSet<Color>();
    GetColors(colors, document);

    var colorGroups = colors.GroupBy(MapColor).ToArray();

    foreach (var group in colorGroups)
    {
      var doc = (SvgDocument)ToGroupIfNeeded((SvgDocument)document.DeepCopy<SvgDocument>(), new HashSet<Color>(group));
      doc = await _removeEmptyGroups.OptimizeAsync(doc);

      if (group.Key == 0)
      {
        yield return ((name == "" ? "" : name + "-") + "cut", document: doc);
      }
      else
      {
        yield return ((name == "" ? "" : name + "-") + "power-" + (int)((255 - group.Key) / 255d * 100d), document: doc);
      }
    }
  }

  private static void GetColors(HashSet<Color> colors, SvgElement element)
  {
    foreach (var child in element.Children)
    {
      GetColors(colors, child);
    }

    if (element.Stroke is not SvgColourServer colourServer) return;

    colors.Add(colourServer.Colour);
  }

  private static int MapColor(Color color)
  {
    return (int)(color.R * .3 + color.G * .59 + color.B * .11);
  }

  private static SvgElement ToGroupIfNeeded(SvgElement element, HashSet<Color> colors)
  {
    var children = element.Children.ToArray();
    element.Children.Clear();

    foreach (var child in children)
    {
      element.Children.Add(ToGroupIfNeeded(child, colors));
    }

    if (element is not SvgVisualElement) return element;

    if (element is SvgGroup) return element;

    if (element.Stroke is SvgColourServer colourServer && colors.Contains(colourServer.Colour)) return element;

    var group = new SvgGroup();
    if (element.Transforms != null)
    {
      group.Transforms = [];
      foreach (var transform in element.Transforms)
      {
        group.Transforms.Add(transform);
      }
    }

    foreach (var child in element.Children)
    {
      group.Children.Add(child);
    }

    return group;
  }
}

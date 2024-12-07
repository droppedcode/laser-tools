using DroppedCode.LaserOptimizer.Core.Optimizers;
using ExCSS;
using Svg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DroppedCode.LaserOptimizer.Core.PreProcessors;
public class LayerSplitter : IPreProcessor
{
  public int Order => 900;

  public async IAsyncEnumerable<(string name, SvgDocument document)> ProcessAsync(string name, SvgDocument document)
  {
    var layers = new HashSet<string>();
    GetLayers(document, layers);

    if (layers.Count > 1)
    {
      foreach (var layer in layers)
      {
        var doc = (SvgDocument)document.DeepCopy<SvgDocument>();

        FilterToLayer(doc, layer);

        yield return ((name == "" ? "" : name + "-") + layer, doc);
      }
    }
    else
    {
      yield return (name, document);
    }
  }

  private static void GetLayers(SvgElement element, HashSet<string> layers)
  {
    if (
      element is SvgGroup group
      && group.TryGetAttribute("http://www.inkscape.org/namespaces/inkscape:groupmode", out var groupMode)
      && groupMode == "layer"
      && group.TryGetAttribute("http://www.inkscape.org/namespaces/inkscape:label", out var label)
    )
    {
      layers.Add(label);
    }

    foreach (var child in element.Children)
    {
      GetLayers(child, layers);
    }
  }

  private static void FilterToLayer(SvgElement element, string layer)
  {
    for (var i = element.Children.Count - 1; i >= 0; i--)
    {
      var child = element.Children[i];
      if (
        child is SvgGroup group
        && group.TryGetAttribute("http://www.inkscape.org/namespaces/inkscape:groupmode", out var groupMode)
        && groupMode == "layer"
        && group.TryGetAttribute("http://www.inkscape.org/namespaces/inkscape:label", out var label)
        && label == layer
        )
      {
        continue;
      }

      FilterToLayer(child, layer);

      if (child is SvgVisualElement && child.Children.Count == 0)
      {
        element.Children.RemoveAt(i);
      }
    }
  }

}

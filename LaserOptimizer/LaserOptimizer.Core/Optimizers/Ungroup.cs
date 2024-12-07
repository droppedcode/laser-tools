using Svg;
using Svg.Transforms;

namespace DroppedCode.LaserOptimizer.Core.Optimizers;

public class Ungroup : IOptimizer
{
  public int Order => 2000;

  public Task<SvgDocument> OptimizeAsync(SvgDocument document)
  {
    UngroupElement(document);

    return Task.FromResult(document);
  }

  private static void UngroupElement(SvgElement element)
  {
    for (var i = 0; i < element.Children.Count; i++)
    {
      var child = element.Children[i];
      UngroupElement(child);

      if (child is SvgGroup)
      {
        var grandChildren = child.Children.ToArray();
        child.Children.Clear();

        var matrix = child.Transforms?.GetMatrix();

        element.Children.RemoveAt(i);

        foreach (var grandChild in grandChildren)
        {
          if (matrix is not null)
          {
            if (grandChild.Transforms?.Count > 0)
            {
              var gcMatrix = grandChild.Transforms.GetMatrix();
              gcMatrix.Multiply(matrix);
              grandChild.Transforms.Clear();
              grandChild.Transforms.Add(new SvgMatrix([.. gcMatrix.Elements]));
            }
            else
            {
              grandChild.Transforms = [];
              grandChild.Transforms.AddRange(child.Transforms);
            }
          }

          if (i == element.Children.Count)
          {
            element.Children.Add(grandChild);
          }
          else
          {
            element.Children.Insert(i, grandChild);
          }
          i++;
        }

        i--;
      }
    }
  }
}

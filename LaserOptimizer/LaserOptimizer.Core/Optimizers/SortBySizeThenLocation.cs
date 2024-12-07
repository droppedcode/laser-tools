using DroppedCode.LaserOptimizer.Core.Extensions;
using Svg;
using System.Drawing;

namespace DroppedCode.LaserOptimizer.Core.Optimizers;

public class SortBySizeThenLocation : IOptimizer
{
  public int Order => 1000;

  public Task<SvgDocument> OptimizeAsync(SvgDocument document)
  {
    Sort(document.Children);

    return Task.FromResult(document);
  }

  private static void Sort(SvgElementCollection collection)
  {
    if (collection.Count == 0)
    {
      return;
    }

    foreach (var element in collection)
    {
      Sort(element.Children);
    }

    var boundables = collection.Select((s, i) => new OrderableBoundable(i, s as ISvgBoundable, s)).ToList();

    var nonSortable = boundables.Where(w => w.Boundable is null);
    var rootChildren = boundables.Where(w => w.Boundable is not null).ToList();
    rootChildren.Sort();

    collection.Clear();

    foreach (var boundable in nonSortable)
    {
      collection.Add(boundable.Element);
    }
    foreach (var boundable in rootChildren)
    {
      collection.Add(boundable.Element);
    }
  }

  private class OrderableBoundable(int OriginalIndex, ISvgBoundable? Boundable, SvgElement Element) : IComparable<OrderableBoundable>
  {
    public int OriginalIndex { get; } = OriginalIndex;

    public ISvgBoundable? Boundable { get; } = Boundable;

    public SvgElement Element { get; } = Element;

    public int CompareTo(OrderableBoundable? other)
    {
      if (other is null)
      {
        return 1;
      }
      else
      {
        var x = Boundable;
        var y = other.Boundable;

        if (x is null && y is null)
        {
          return 0;
        }
        else if (x is null || y is null)
        {
          return x is null ? -1 : 1;
        }
        else if (x.Size.IsSimilar(y.Size))
        {
          if (x.Location.Y.IsSimilar(y.Location.Y))
          {
            return (int)(x.Location.X - y.Location.X);
          }
          else
          {
            return (int)(x.Location.Y - y.Location.Y);
          }

          //return LocationCSquared(x.Location) - LocationCSquared(y.Location);
        }
        else
        {
          return (int)(x.Size.ToVector2().LengthSquared() - y.Size.ToVector2().LengthSquared());
        }
      }
    }

    private static int LocationCSquared(PointF point)
    {
      var x = point.X;
      if (x < 0)
      {
        x = 0;
      }
      var y = point.Y;
      if (y < 0)
      {
        y = 0;
      }

      return (int)(x * x + y * y);
    }
  }
}

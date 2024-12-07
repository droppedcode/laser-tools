using Svg;
using System.Drawing;

namespace DroppedCode.LaserOptimizer.Core.Optimizers;
public class GroupWithin : IOptimizer
{
  private static readonly SizeF _containsTopLeftOffset = new(1f, 1f);
  private static readonly SizeF _containsBottomRightOffset = new(2f, 2f);

  public int Order => 500;

  public Task<SvgDocument> OptimizeAsync(SvgDocument document)
  {
    GroupElements(document.Children);

    return Task.FromResult(document);
  }

  private static void GroupElements(SvgElementCollection collection)
  {
    if (collection.Count == 0)
    {
      return;
    }

    foreach (var element in collection)
    {
      GroupElements(element.Children);
    }

    var boundables = collection.Select((s, i) => new OrderableBoundable(i, s as ISvgBoundable, s)).ToList();

    foreach (var element in boundables)
    {
      foreach (var boundable in boundables)
      {
        if (element == boundable) continue;

        element.SetContainer(boundable);
      }
    }

    collection.Clear();

    foreach (var boundable in boundables.Where(w => w.Container is null))
    {
      collection.Add(boundable.ToGroupIfNeeded());
    }
  }

  private class OrderableBoundable(int OriginalIndex, ISvgBoundable? Boundable, SvgElement Element)
  {
    public int OriginalIndex { get; } = OriginalIndex;

    public ISvgBoundable? Boundable { get; } = Boundable;

    public OrderableBoundable? Container { get; private set; }

    public List<OrderableBoundable> Children { get; } = [];

    public SvgElement Element { get; } = Element;

    public void SetContainer(OrderableBoundable boundable)
    {
      if (Boundable is null || boundable.Boundable is null) return;

      if (boundable.Contains(Boundable) && (Container is null || Container.Contains(boundable.Boundable)))
      {
        Container?.Children.Remove(this);
        Container = boundable;
        Container.Children.Add(this);
      }
    }

    public bool Contains(ISvgBoundable other)
    {
      return Boundable is not null
      && Boundable.Bounds.Contains(other.Location + _containsTopLeftOffset)
      && Boundable.Bounds.Contains(other.Location + other.Size - _containsBottomRightOffset);
    }

    public SvgElement ToGroupIfNeeded()
    {
      if (Children.Count == 0) return Element;

      var group = new SvgGroup();

      group.Children.Add(Element);

      foreach (var item in Children)
      {
        group.Children.Add(item.ToGroupIfNeeded());
      }

      return group;
    }
  }
}

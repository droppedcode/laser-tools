using System.Drawing;

namespace DroppedCode.LaserOptimizer.Core.Extensions;
public static class MathFExtensions
{
  private const float Epsilon = 0.001f;

  public static bool IsSimilar(this float self, float other)
  {
    return Math.Abs(self - other) <= Epsilon;
  }

  public static bool IsSimilar(this SizeF self, SizeF other)
  {
    return self.Width.IsSimilar(other.Width) && self.Height.IsSimilar(other.Height);
  }
}

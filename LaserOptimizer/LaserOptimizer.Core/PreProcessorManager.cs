using DroppedCode.LaserOptimizer.Core.PreProcessors;
using Svg;

namespace DroppedCode.LaserOptimizer.Core;
public class PreProcessorManager
{
  private IPreProcessor[] _preProcessors;

  public PreProcessorManager(IEnumerable<IPreProcessor> preProcessors) => _preProcessors = preProcessors.OrderBy(o => o.Order).ToArray();

  public async IAsyncEnumerable<(string name, SvgDocument document)> ProcessAsync(SvgDocument document)
  {
    List<(string name, SvgDocument document)> documents = [("", document)];

    foreach (var processor in _preProcessors)
    {
      List<(string name, SvgDocument document)> nexts = [];

      foreach (var doc in documents)
      {
        await foreach (var next in processor.ProcessAsync(doc.name, doc.document))
        {
          nexts.Add(next);
        }
      }

      documents = nexts;
    }

    foreach (var doc in documents)
    {
      yield return doc;
    }
  }
}

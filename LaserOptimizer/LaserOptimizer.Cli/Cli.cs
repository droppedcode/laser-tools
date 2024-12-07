using DroppedCode.LaserOptimizer.Core;
using Svg;

namespace DroppedCode.LaserOptimizer.Cli;
internal class Cli
{
  private readonly OptimizerManager _optimizer;
  private readonly PreProcessorManager _preProcessor;

  public Cli(OptimizerManager optimizer, PreProcessorManager preProcessor)
  {
    _optimizer = optimizer;
    _preProcessor = preProcessor;
  }

  public async Task RunAsync(CliSettings settings)
  {
    var files = CollectFiles(settings);

    if (files == null)
    {
      return;
    }

    await Task.WhenAll(files.Select(async (inputFile, index) =>
    {
      var outputPath = string.Empty;

      var originalDocument = SvgDocument.Open(inputFile.FullName);

      var documents = new List<(string name, SvgDocument document)>();

      await foreach (var (name, document) in _preProcessor.ProcessAsync(originalDocument))
      {
        documents.Add((name, await _optimizer.OptimizeAsync(document)));
      }

      foreach (var document in documents)
      {
        outputPath = Path.ChangeExtension(inputFile.FullName, "." + (document.name != "" ? document.name : "optimized") + inputFile.Extension);

        if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
        {
          outputPath = Path.Combine(settings.OutputDirectory.FullName, Path.GetFileName(outputPath));
        }

        document.document.Write(outputPath);
      }
    }));
  }

  static void GetFiles(DirectoryInfo directory, string pattern, List<FileInfo> paths)
  {
    var files = Directory.EnumerateFiles(directory.FullName, pattern);
    if (files != null)
    {
      foreach (var path in files)
      {
        paths.Add(new FileInfo(path));
      }
    }
  }

  private static List<FileInfo>? CollectFiles(CliSettings settings)
  {
    var paths = new List<FileInfo>();

    if (settings.InputFiles != null)
    {
      foreach (var file in settings.InputFiles)
      {
        paths.Add(file);
      }
    }

    if (settings.InputDirectory != null)
    {
      var directory = settings.InputDirectory;
      GetFiles(directory, "*.svg", paths);
      GetFiles(directory, "*.svgz", paths);
    }

    if (settings.OutputDirectory != null && !string.IsNullOrEmpty(settings.OutputDirectory.FullName))
    {
      if (!Directory.Exists(settings.OutputDirectory.FullName))
      {
        Directory.CreateDirectory(settings.OutputDirectory.FullName);
      }
    }

    return paths;
  }
}

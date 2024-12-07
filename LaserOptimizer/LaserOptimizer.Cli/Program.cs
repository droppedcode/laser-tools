using DroppedCode.LaserOptimizer.Cli.Extensions;
using DroppedCode.LaserOptimizer.Core;
using DroppedCode.LaserOptimizer.Core.Optimizers;
using DroppedCode.LaserOptimizer.Core.PreProcessors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

namespace DroppedCode.LaserOptimizer.Cli;

internal class Program
{
  private static async Task<int> Main(string[] args)
  {
    var optionInputFiles = new Option<FileInfo[]?>(["--inputFiles", "-f"], "The relative or absolute path to the input files");
    var optionInputDirectory = new Option<DirectoryInfo?>(["--inputDirectory", "-d"], "The relative or absolute path to the input directory");
    var optionOutputDirectory = new Option<DirectoryInfo?>(["--outputDirectory", "-o"], "The relative or absolute path to the output directory");

    var rootCommand = new RootCommand()
    {
      Description = "Optimizes svg files for laser cutting."
    };

    rootCommand.AddOption(optionInputFiles);
    rootCommand.AddOption(optionInputDirectory);
    rootCommand.AddOption(optionOutputDirectory);

    rootCommand.SetHandler(async (host, inputFiles, inputDirectory, outputDirector) =>
    {
      var logger = host.Services.GetRequiredService<ILogger<Program>>();

      try
      {
        var cli = host.Services.GetRequiredService<Cli>();

        await cli.RunAsync(new CliSettings
        {
          InputDirectory = inputDirectory,
          InputFiles = inputFiles == null || inputFiles.Length == 0 ? null : inputFiles,
          OutputDirectory = outputDirector,
        });
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "Error during program execution.");
      }
    },
    new Injectable<IHost>(),
    optionInputFiles,
    optionInputDirectory,
    optionOutputDirectory);

    var parser = new CommandLineBuilder(rootCommand)
      .UseDefaults()
      .UseHost(Host.CreateDefaultBuilder, host =>
      {
        host.ConfigureServices(services =>
        {
          services.AddLogging(logging => logging.AddConsole());

          services.AddSingleton<Cli>();

          services.AddSingleton<OptimizerManager>();

          services.AddSingletonWithImplementation<IOptimizer, RemoveEmptyGroup>();
          services.AddSingletonWithImplementation<IOptimizer, GroupWithin>();
          services.AddSingletonWithImplementation<IOptimizer, SortBySizeThenLocation>();
          services.AddSingletonWithImplementation<IOptimizer, Ungroup>();

          services.AddSingleton<PreProcessorManager>();

          services.AddSingletonWithImplementation<IPreProcessor, LayerSplitter>();
          services.AddSingletonWithImplementation<IPreProcessor, ColorSplitter>();
        });
      })
      .Build();

    return await parser.Parse(args).InvokeAsync();
  }
}
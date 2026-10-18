using System.CommandLine;
using CollateWallpaperCLI.Handlers;

namespace CollateWallpaperCLI.CLI;

public class RootCommandFactory
{
    private readonly Option<DirectoryInfo> _inputDirectoryOption = new("--input-directory", "-i")
    {
        Description = "The input directory to scan",
        Required = true
    };

    private readonly Option<DirectoryInfo> _outputDirectoryOption = new("--output-directory", "-o")
    {
        Description = "The output directory for copies (created automatically if missing)",
        Required = true
    };

    private readonly Option<bool> _quietSkip = new("--quiet-skip", "-q")
    {
        Description = "Quietly skip any error while scanning and copying (off by default)"
    };

    private readonly Option<bool> _recursiveOption = new("--recursive", "-r")
    {
        Description = "Search subdirectories recursively (off by default)"
    };

    private readonly Option<bool> _withCover = new("--with-cover", "-c")
    {
        Description = "Make the video cover (off by default) (TODO)"
    };

    private RootCommandFactory()
    {
        Root.Options.Add(_inputDirectoryOption);
        Root.Options.Add(_outputDirectoryOption);
        Root.Options.Add(_recursiveOption);
        Root.Options.Add(_withCover);
        Root.Options.Add(_quietSkip);

        Root.SetAction(RunAction);
    }

    public static RootCommandFactory Default => field ??= new RootCommandFactory();

    public RootCommand Root { get; init; } = new();

    private int RunAction(ParseResult parseResult)
    {
        var input = parseResult.GetValue(_inputDirectoryOption)!;
        var output = parseResult.GetValue(_outputDirectoryOption)!;
        var recursive = parseResult.GetValue(_recursiveOption);
        var withCover = parseResult.GetValue(_withCover);
        var quietSkip = parseResult.GetValue(_quietSkip);

        return ScanHandler.Default.Run(input, output, recursive, withCover, quietSkip);
    }

    public int ParseAndRun(string[] args)
    {
        return Root.Parse(args).Invoke();
    }
}

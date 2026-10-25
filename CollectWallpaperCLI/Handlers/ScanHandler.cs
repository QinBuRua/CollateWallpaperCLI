using CollectWallpaperCLI.Services;

namespace CollectWallpaperCLI.Handlers;

public static class ScanHandler
{
    public static int Run(
        DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        bool recursive,
        bool withCover,
        bool quietSkip)
    {
        try
        {
            var scanner = new Scanner
            {
                InputDirectory = inputDirectory,
                OutputDirectory = outputDirectory,
                Recursive = recursive,
                WithCover = withCover,
                QuietSkip = quietSkip
            };

            if (!scanner.Run(out var expectedException)) Console.Error.WriteLine($"Meeting an exception while preparing scan: {expectedException}");

            Console.WriteLine($"Copy {scanner.SuccessCount} files. Skip {scanner.SkipCount} projects. Met {scanner.WarnCount} warnings and {scanner.ErrorCount} errors.");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unexpected Exception: {e.Message}");
            return e.HResult;
        }

        return 0;
    }
}

using CollateWallpaperCLI.Services;

namespace CollateWallpaperCLI.Handlers;

public class ScanHandler
{
    public static ScanHandler Default => field ??= new ScanHandler();

    public int Run(
        DirectoryInfo inputDirectory,
        DirectoryInfo outputDirectory,
        bool recursive,
        bool withCover)
    {
        try
        {
            var scanner = new Scanner
            {
                InputDirectory = inputDirectory,
                OutputDirectory = outputDirectory,
                Recursive = recursive,
                WithCover = withCover
            };
            
            scanner.Run();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unexpected Exception: {e.Message}");
            return e.HResult;
        }

        return 0;
    }
}

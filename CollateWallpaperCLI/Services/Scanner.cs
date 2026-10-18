using System.Diagnostics.CodeAnalysis;
using CollateWallpaperCLI.Infrastructure;

namespace CollateWallpaperCLI.Services;

public enum ScannerRunException
{
    InputDirectoryNotFound,
    OutputDirectoryOccupied
}

public class Scanner
{
    private const string ProjectFileName = "project.json";
    public required DirectoryInfo InputDirectory { get; set; }
    public required DirectoryInfo OutputDirectory { get; set; }
    public required bool Recursive { get; set; }
    public required bool WithCover { get; set; }

    public uint SkipCount { get; set; }
    public uint ErrorCount { get; set; }
    public uint SuccessCount { get; set; }

    public bool Run([NotNullWhen(false)] out ScannerRunException? expectedException)
    {
        if (!Directory.Exists(InputDirectory.FullName))
        {
            expectedException = ScannerRunException.InputDirectoryNotFound;
            return false;
        }

        if (File.Exists(OutputDirectory.FullName))
        {
            expectedException = ScannerRunException.OutputDirectoryOccupied;
            return false;
        }

        Scan(InputDirectory);
        
        expectedException = null;
        return true;
    }

    private void Scan(DirectoryInfo directory)
    {
        var projectPath = Path.Combine(directory.FullName, ProjectFileName);

        if (!File.Exists(projectPath))
        {
            if (!Recursive) return;

            foreach (var subDirectory in directory.EnumerateDirectories()) Scan(subDirectory);
        }
        else
        {
            var processResult = ProcessProjectHelper.ProcessProject(new FileInfo(projectPath));

            switch (processResult.Type)
            {
                case ProcessResultType.Skip:
                    SkipCount++;
                    return;
                case ProcessResultType.Copy:
                    var result = VideoCopyHelper.Copy(processResult.SourceFile!, processResult.TargetFileName!, OutputDirectory);
                    if (result == CopyResultType.Success)
                    {
                        SuccessCount++;
                        return;
                    }

                    Console.Error.WriteLine($"Missing Error \"{result}\" while copying {processResult.SourceFile} to {Path.Combine(OutputDirectory.FullName, processResult.TargetFileName!)}.");
                    return;
                case ProcessResultType.InvalidJson:
                case ProcessResultType.InvalidProjectFormat:
                default:
                    Console.Error.WriteLine($"Missing Error \"{processResult.Type}\" while processing \"{projectPath}\"");
                    ErrorCount++;
                    return;
            }
        }
    }
}

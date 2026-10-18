using CollateWallpaperCLI.Infrastructure;

namespace CollateWallpaperCLI.Services;

public class Scanner
{
    private const string ProjectFileName = "project.json";
    public required DirectoryInfo InputDirectory { get; set; }
    public required DirectoryInfo OutputDirectory { get; set; }
    public required bool Recursive { get; set; }
    public required bool WithCover { get; set; }

    public uint SkipCount { get; set; }
    public uint SuccessCount { get; set; }

    public void Run()
    {
        if (!InputDirectory.Exists) throw new DirectoryNotFoundException(InputDirectory.Name);

        if (FileSystemHelper.ExistFileOrDirectory(OutputDirectory.FullName)) throw new IOException($"{OutputDirectory.Name} already exists a file or directory.");

        Scan(InputDirectory);
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

            if (processResult.Type == ProcessResultType.Skip)
            {
                SkipCount++;
                return;
            }
            
            
        }
    }
}

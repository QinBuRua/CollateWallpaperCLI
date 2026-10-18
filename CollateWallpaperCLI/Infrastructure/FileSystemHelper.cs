namespace CollateWallpaperCLI.Infrastructure;

public static class FileSystemHelper
{
    public static bool ExistFileOrDirectory(string path)
    {
        return Directory.Exists(path) || File.Exists(path);
    }
}

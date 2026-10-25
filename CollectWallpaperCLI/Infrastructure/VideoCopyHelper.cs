namespace CollectWallpaperCLI.Infrastructure;

public enum CopyResultType
{
    Success,
    MissingSourceFile,
    TargetFileHasExisted,
    FailToCreateDirectory,
    UnknownIOError
}

public static class VideoCopyHelper
{
    public static CopyResultType Copy(FileInfo sourceFile, FileInfo? coverFile, string targetFileName, DirectoryInfo targetDirectory) //todo
    {
        try
        {
            if (!File.Exists(sourceFile.FullName)) return CopyResultType.MissingSourceFile;

            var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, targetFileName));
            if (File.Exists(targetFile.FullName)) return CopyResultType.TargetFileHasExisted;

            if (!Directory.Exists(targetDirectory.FullName))
                try
                {
                    Directory.CreateDirectory(targetDirectory.FullName);
                }
                catch (Exception _)
                {
                    return CopyResultType.FailToCreateDirectory;
                }

            File.Copy(sourceFile.FullName, targetFile.FullName, false);

            return CopyResultType.Success;
        }
        catch (IOException _)
        {
            return CopyResultType.UnknownIOError;
        }
    }
}

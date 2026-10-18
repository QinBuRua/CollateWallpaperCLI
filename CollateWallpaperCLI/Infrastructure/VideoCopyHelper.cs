namespace CollateWallpaperCLI.Infrastructure;

public enum CopyResultType
{
    Success,
    MissingSourceFile,
    TargetFileHasExisted,
    UnknownIOError
}

public static class VideoCopyHelper
{
    public static CopyResultType Copy(FileInfo sourceFile, string targetFileName, DirectoryInfo targetDirectory)
    {
        try
        {
            if (!sourceFile.Exists) return CopyResultType.MissingSourceFile;

            var targetFile = new FileInfo(Path.Combine(targetDirectory.FullName, targetFileName));
            if (targetFile.Exists) return CopyResultType.TargetFileHasExisted;

            File.Copy(sourceFile.FullName, targetFile.FullName, false);

            return CopyResultType.Success;
        }
        catch (IOException e)
        {
            return CopyResultType.UnknownIOError;
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CollateWallpaperCLI.Infrastructure;

public class ProcessProjectHelper
{
    private static Dictionary<string, string?> ReadFields(JsonElement root)
    {
        var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.EnumerateObject())
            fields[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.ToString();

        return fields;
    }

    private static bool ReadProjectFile(FileInfo projectFile,
        [NotNullWhen(true)] out Dictionary<string, string?>? jsonDic)
    {
        using var document = JsonDocument.Parse(projectFile.OpenRead());

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            jsonDic = null;
            return false;
        }

        jsonDic = ReadFields(document.RootElement);
        return true;
    }

    private static bool ShouldCopy(Dictionary<string, string?> jsonDic)
    {
        var isVideo = string.Equals(jsonDic["type"], "video", StringComparison.OrdinalIgnoreCase);
        var isAdult = string.Equals(jsonDic["type"], "mature", StringComparison.OrdinalIgnoreCase);
        return isAdult && isVideo;
    }

    private static bool GetFileTitlePair(Dictionary<string, string?> jsonDic,
        [NotNullWhen(true)] out string? sourceFileName,
        [NotNullWhen(true)] out string? title)
    {
        sourceFileName = jsonDic.GetValueOrDefault("file");
        title = jsonDic.GetValueOrDefault("title");

        return !string.IsNullOrWhiteSpace(sourceFileName) && !string.IsNullOrWhiteSpace(title);
    }

    private static (FileInfo SourceFile, string TargetFileName) ProcessSourceTargetFilePair(FileInfo projectFile, string sourceFileName, string title)
    {
        var directory = Path.GetDirectoryName(projectFile.FullName)!;
        var sourceFilePath = Path.IsPathRooted(sourceFileName) ? sourceFileName : Path.Combine(directory, sourceFileName);

        var targetFileName = Path.Combine(title, Path.GetExtension(sourceFileName));
        if (targetFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) targetFileName = sourceFileName;

        return (new FileInfo(sourceFilePath), targetFileName);
    }

    public static ProcessResult ProcessProject(FileInfo projectFile)
    {
        if (!ReadProjectFile(projectFile, out var jsonDic)) return ProcessResult.MakeResult(ProcessResultType.InvalidJson);

        if (!ShouldCopy(jsonDic)) return ProcessResult.MakeResult(ProcessResultType.Skip);

        if (!GetFileTitlePair(jsonDic, out var sourceFileName, out var title)) return ProcessResult.MakeResult(ProcessResultType.InvalidProjectFormat);

        var sourceTargetFilePair = ProcessSourceTargetFilePair(projectFile, sourceFileName, title);

        return ProcessResult.MakeResult(ProcessResultType.Copy, sourceTargetFilePair.SourceFile, sourceTargetFilePair.TargetFileName);
    }
}

public enum ProcessResultType
{
    Copy,
    Skip,
    InvalidJson,
    InvalidProjectFormat
}

public record ProcessResult(FileInfo? SourceFile, string? TargetFileName)
{
    public ProcessResultType Type { get; init; }

    public bool TryGetInputFileInfo([NotNullWhen(true)] out FileInfo? sourceFileInfo)
    {
        sourceFileInfo = SourceFile;
        return SourceFile != null && Type == ProcessResultType.Copy;
    }

    public bool TryGetTargetFileInfo([NotNullWhen(true)] out string? targetFileName)
    {
        targetFileName = TargetFileName;
        return TargetFileName != null && Type == ProcessResultType.Copy;
    }

    public bool TryGetInputTargetPair([NotNullWhen(true)] out FileInfo? sourceFile, [NotNullWhen(true)] out string? targetFileName)
    {
        sourceFile = SourceFile;
        targetFileName = TargetFileName;
        return Type == ProcessResultType.Copy && sourceFile != null && targetFileName != null;
    }

    public static ProcessResult MakeResult(ProcessResultType resultType, FileInfo? sourceFile = null, string? targetFileName = null)
    {
        if (resultType == ProcessResultType.Copy)
            return new ProcessResult(sourceFile!, targetFileName!)
            {
                Type = ProcessResultType.Copy
            };

        return new ProcessResult(null, null)
        {
            Type = resultType
        };
    }
}

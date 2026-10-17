using System.Text.Json;

namespace CollateWallpaperCLI;

/// <summary>
///     扫描目录，查找满足条件的 project.json，并把对应的视频复制到输出目录。
/// </summary>
internal sealed class ProjectScanner
{
    private const string ProjectFileName = "project.json";

    private readonly DirectoryInfo _output;

    public ProjectScanner(DirectoryInfo output)
    {
        _output = output;
    }

    /// <summary>成功复制的文件数量。</summary>
    public int CopiedCount { get; private set; }

    /// <summary>因各种异常被跳过的数量。</summary>
    public int SkippedCount { get; private set; }

    /// <summary>
    ///     从指定目录开始扫描。
    /// </summary>
    /// <param name="directory">要扫描的目录。</param>
    /// <param name="recursive">是否递归搜索子目录。</param>
    public void Scan(DirectoryInfo directory, bool recursive)
    {
        var projectPath = Path.Combine(directory.FullName, ProjectFileName);

        if (File.Exists(projectPath))
        {
            // 一旦发现 project.json，就处理它，并且不再深入它的子目录
            ProcessProject(projectPath);
            return;
        }

        // 未发现 project.json：只有开启递归才继续向下遍历
        if (!recursive) return;

        foreach (var subDirectory in directory.EnumerateDirectories()) Scan(subDirectory, recursive);
    }

    /// <summary>
    ///     处理单个 project.json。
    /// </summary>
    private void ProcessProject(string projectPath)
    {
        Dictionary<string, string?> fields;

        // ===== 1. 读取并解析 JSON（字段名不区分大小写）=====
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(projectPath));

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Warn(projectPath, "the root of project.json is not a JSON object");
                return;
            }

            fields = ReadFields(document.RootElement);
        }
        catch (JsonException)
        {
            Warn(projectPath, "project.json is not valid JSON, skipped");
            return;
        }
        catch (IOException ex)
        {
            Warn(projectPath, $"failed to read project.json: {ex.Message}");
            return;
        }

        // ===== 2. 判断条件：type=video 且 ratingsex=adult（值不区分大小写）=====
        var isVideo = string.Equals(fields.GetValueOrDefault("type"), "video", StringComparison.OrdinalIgnoreCase);
        var isAdult = string.Equals(fields.GetValueOrDefault("contentrating"), "Mature", StringComparison.OrdinalIgnoreCase);

        if (!isVideo || !isAdult)
            // 条件不满足，静默跳过
            return;

        // ===== 3. 取出 file / title 字段 =====
        var file = fields.GetValueOrDefault("file");
        var title = fields.GetValueOrDefault("title");

        if (string.IsNullOrWhiteSpace(file))
        {
            Warn(projectPath, "conditions met, but the file field is missing or empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            Warn(projectPath, "conditions met, but the title field is missing or empty");
            return;
        }

        // ===== 4. 解析源文件路径（相对 project.json 所在目录）=====
        var directory = Path.GetDirectoryName(projectPath)!;
        var sourcePath = Path.IsPathRooted(file) ? file : Path.Combine(directory, file);

        if (!File.Exists(sourcePath))
        {
            Warn(projectPath, $"the file referenced by 'file' does not exist: {sourcePath}");
            return;
        }

        // ===== 5. 计算目标文件名：title + file 的扩展名 =====
        var extension = Path.GetExtension(sourcePath);
        var targetName = title + extension;

        // title 含非法文件名字符时，警告并回退为源文件名
        if (targetName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Warn(projectPath, "title contains invalid file name characters, using the file name instead");
            targetName = Path.GetFileName(sourcePath);
        }

        // ===== 6. 复制到输出目录（平铺，不保留原有层级）=====
        var targetPath = Path.Combine(_output.FullName, targetName);

        if (File.Exists(targetPath))
        {
            Warn(projectPath, $"a file with the same name already exists in the output directory, skipped: {targetPath}");
            return;
        }

        try
        {
            File.Copy(sourcePath, targetPath);
            CopiedCount++;
        }
        catch (Exception ex)
        {
            Warn(projectPath, $"copy failed: {ex.Message}");
        }
    }

    /// <summary>
    ///     把根对象的所有字段读入一个“字段名不区分大小写”的字典。
    /// </summary>
    private static Dictionary<string, string?> ReadFields(JsonElement root)
    {
        var fields = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in root.EnumerateObject())
            fields[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString()
                : property.Value.ToString();

        return fields;
    }

    /// <summary>
    ///     输出警告、计数，并把对应的 project.json 路径一并打印。
    /// </summary>
    private void Warn(string projectPath, string message)
    {
        SkippedCount++;
        Console.Error.WriteLine($"[Warning] {message} (project: {projectPath})");
    }
}

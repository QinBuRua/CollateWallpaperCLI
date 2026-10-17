using System.CommandLine;

using CollateWallpaperCLI;

// ===== 命令行定义：两个必填选项 + 一个开关 =====

// 选项 1：输入目录（必填）
var inputOption = new Option<DirectoryInfo>("--input-directory", "-i")
{
    Description = "The input directory to scan",
    Required = true
};

// 选项 2：输出目录（必填，不存在会自动创建）
var outputOption = new Option<DirectoryInfo>("--output-directory", "-o")
{
    Description = "The output directory for copies (created automatically if missing)",
    Required = true
};

// 开关：是否递归搜索，默认关闭（纯开关，出现即开启）
var recursiveOption = new Option<bool>("--recursive", "-r")
{
    Description = "Search subdirectories recursively (off by default)"
};

// 根命令
var rootCommand = new RootCommand("Find matching videos in a directory and copy them to the output directory");
rootCommand.Options.Add(inputOption);
rootCommand.Options.Add(outputOption);
rootCommand.Options.Add(recursiveOption);

// 绑定处理逻辑
rootCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(inputOption)!;
    var output = parseResult.GetValue(outputOption)!;
    var recursive = parseResult.GetValue(recursiveOption);

    return Run(input, output, recursive);
});

// 解析参数并执行
return rootCommand.Parse(args).Invoke();

// ===== 入口处理 =====
static int Run(DirectoryInfo input, DirectoryInfo output, bool recursive)
{
    // 输入目录必须存在
    if (!input.Exists)
    {
        Console.Error.WriteLine($"Error: input directory does not exist: {input.FullName}");
        return 1;
    }

    // 输出目录不存在则自动创建
    Directory.CreateDirectory(output.FullName);

    // 开始扫描
    var scanner = new ProjectScanner(output);
    scanner.Scan(input, recursive);

    // 仅输出汇总结果，不逐个打印已复制项
    Console.WriteLine($"Done: {scanner.CopiedCount} file(s) copied, {scanner.SkippedCount} skipped.");
    return 0;
}

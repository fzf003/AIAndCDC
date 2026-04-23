namespace CdcRedisConsumer;

/// <summary>
/// 轻量 .env 文件加载器，将 KEY=VALUE 注入进程环境变量。
/// 不引入第三方依赖，支持注释（#）和空行过滤。
/// </summary>
public static class DotEnvLoader
{
    /// <summary>
    /// 加载指定路径的 .env 文件到进程环境变量。
    /// 若文件不存在则静默跳过。
    /// </summary>
    /// <param name="path">.env 文件路径，默认为当前目录下的 .env</param>
    public static void Load(string path = ".env")
    {
        var file = new FileInfo(path);
        if (!file.Exists) return;

        foreach (var line in File.ReadLines(file.FullName))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;

            var separator = trimmed.IndexOf('=');
            if (separator <= 0) continue;

            var key = trimmed[..separator].Trim();
            var value = trimmed[(separator + 1)..].Trim();

            // 仅当进程环境变量未设置时才从 .env 注入
            // 优先级：系统环境变量 > .env 文件
            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}

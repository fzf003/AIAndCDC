using System.Text.RegularExpressions;

namespace CdcRedisConsumer;

/// <summary>
/// IConfiguration 扩展方法，支持 ${VAR} 占位符的环境变量替换。
/// </summary>
public static class ConfigurationExtensions
{
    private static readonly Regex EnvVarPattern = new(
        @"\$\{(\w+)\}",
        RegexOptions.Compiled);

    /// <summary>
    /// 读取配置值并解析其中的 ${VAR} 占位符为对应环境变量值。
    /// 若环境变量不存在，占位符保持原样。
    /// </summary>
    /// <param name="config">配置实例</param>
    /// <param name="key">配置键</param>
    /// <returns>替换后的配置值</returns>
    public static string? ResolveEnv(this IConfiguration config, string key)
    {
        var template = config[key];
        if (template is null) return null;

        return EnvVarPattern.Replace(template, match =>
            Environment.GetEnvironmentVariable(match.Groups[1].Value)
            ?? match.Value);
    }
}

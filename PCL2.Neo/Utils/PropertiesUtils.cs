using System.Collections.Generic;
using System.IO;

namespace PCL2.Neo.Utils;

public static class PropertiesUtils
{
    // TODO 添加缓存
    public static Dictionary<string, string> ReadProperties(string filePath)
    {
        var result = new Dictionary<string, string>();

        if (!File.Exists(filePath))
            throw new FileNotFoundException("文件不存在", filePath);

        foreach (var line in File.ReadAllLines(filePath))
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                continue;

            var parts = trimmedLine.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                result[key] = value;
            }
        }

        return result;
    }
}
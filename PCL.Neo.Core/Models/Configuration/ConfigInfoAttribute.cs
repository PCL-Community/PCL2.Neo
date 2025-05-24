using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Models.Configuration;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ConfigInfoAttribute(string path, string fileName) : Attribute
{
    /// <summary>
    /// 配置项的Json路径
    /// </summary>
    public string Path { get; init; } = path;

    /// <summary>
    /// 配置项的Json文件名
    /// </summary>
    public string FileName { get; init; } = fileName;
}
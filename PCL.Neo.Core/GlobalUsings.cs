// 全局引用，为.NET Standard 2.0项目提供常用命名空间
global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.IO;
global using System.Linq;
global using System.Net;
global using System.Net.Http;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading;
global using System.Threading.Tasks;

// 集合类型
global using Dictionary = System.Collections.Generic.Dictionary<string, object>;
global using StringDictionary = System.Collections.Generic.Dictionary<string, string>;
global using ObjectDictionary = System.Collections.Generic.Dictionary<string, object>;

// 常用类型
global using static System.IO.Path;
global using static System.Environment;
global using static System.String; 
using PCL2.Neo.Models.Minecraft.McVersion.VersionData;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PCL2.Neo.Models.Minecraft.McVersion
{
    public partial class VersionFileParser
    {
        private static GameRulesList GameRulesListParser(JsonElement element)
        {
            var rulesList = new GameRulesList { Rules = [], Value = [] };

            var rules = element.GetProperty("rules").EnumerateArray().ElementAt(0);
            //var values = element.GetProperty("value").EnumerateArray();
            var values = element.GetProperty("value");

            var action   = rules.GetProperty("action").GetString()!;
            var features = rules.GetProperty("features");

            var ruleElement = new GameRulesElements
            {
                Action = action,
                RulesFeature = new RulesFeaturesElements
                {
                    Name = RulesNameRegex().Match(features.GetRawText()).Groups[1].Value,
                    Value = features.GetRawText().Contains("true")
                }
            };

            // add values
            switch (values.ValueKind)
            {
                case JsonValueKind.Array:
                    {
                        foreach (var item in values.EnumerateArray()) rulesList.Value.Add(item.GetString()!);
                        break;
                    }
                case JsonValueKind.String:
                    rulesList.Value.Add(values.GetString()!);
                    break;
                default:
                    throw new JsonException("Unknown JsonValueKind");
            }

            rulesList.Rules.Add(ruleElement);

            return rulesList;
        }

        private static Game GameParser(JsonElement element)
        {
            var enummerable = element.EnumerateArray();
            var game        = new Game { GameRules = [], Value = [] };

            foreach (var item in enummerable)
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.String:
                        game.Value.Add(item.GetString()!);
                        break;
                    case JsonValueKind.Object:
                        game.GameRules.Add(GameRulesListParser(item));
                        break;
                    default:
                        throw new JsonException("Unknown JsonValueKind");
                }
            }

            return game;
        }

        private static JvmRulesElements JvmRulesElementsParser(JsonElement element)
        {
            var os = element.GetProperty("os").GetRawText();

            return new JvmRulesElements
            {
                Action = element.GetProperty("action").GetString()!,
                Os =
                [
                    new OsElements()
                    {
                        Name = RulesNameRegex().Match(os).Groups[1].Value,
                        Value = JvmOsValueRegex().Match(os).Groups[1].Value
                    }
                ]
            };
        }

        private static JvmRulesList JvmRulesListParser(JsonElement element)
        {
            var result = new JvmRulesList { Rules = [], Value = [] };

            var rules  = element.GetProperty("rules").EnumerateArray().ElementAt(0);
            var values = element.GetProperty("value");

            result.Rules.Add(JvmRulesElementsParser(rules));

            switch (values.ValueKind)
            {
                case JsonValueKind.Array:
                    {
                        var valArray = values.EnumerateArray();
                        foreach (var item in valArray) result.Value.Add(item.GetString()!);
                        break;
                    }
                case JsonValueKind.String:
                    result.Value.Add(values.GetString()!);
                    break;
                default:
                    throw new JsonException("Unknown JsonValueKind");
            }

            return result;
        }

        private static Jvm JvmParser(JsonElement element)
        {
            var enummerable = element.EnumerateArray();
            var jvm         = new Jvm { Rules = [], Value = [] };

            foreach (var item in enummerable)
            {
                switch (item.ValueKind)
                {
                    case JsonValueKind.Object:
                        jvm.Rules.Add(JvmRulesListParser(item));
                        break;
                    case JsonValueKind.String:
                        jvm.Value.Add(item.GetString()!);
                        break;
                    default:
                        throw new JsonException("Unknown JsonValueKind");
                }
            }

            return jvm;
        }

        private static Arguments ArgumentsParser(JsonElement element)
        {
            var game = GameParser(element.GetProperty("game"));
            var jvm  = JvmParser(element.GetProperty("jvm"));
            return new Arguments { Game = game, Jvm = jvm };
        }

        private static JvmRulesElements ActionParser(JsonElement element)
        {
            var result = new JvmRulesElements { Os = [], FirstAction = null };
            foreach (var item in element.EnumerateArray())
            {
                if (item.TryGetProperty("os", out _))
                {
                    var action = JvmRulesElementsParser(item);
                    result.Action = action.Action;
                    result.Os.Add(action.Os[0]);
                }
                else
                {
                    var action = item.GetProperty("action").GetString();
                    result.FirstAction = action;
                }
            }

            return result;
        }

        private static (string, string) GetNameValueTuple(string input)
        {
            var match = GetNameValueRegex().Match(input);
            return (match.Groups[1].Value, match.Groups[2].Value);
        }

        private static (string, Artifact) GetClassifiersValueTuple(string element)
        {
            var match   = GetDoubleQuateValue().Match(element);
            var content = ClassifiersContentRegex().Match(element).Value;
            return (match.Groups[1].Value, JsonSerializer.Deserialize<Artifact>(content,
                SerializerOptions)!);
        }

        private static readonly JsonSerializerOptions SerializerOptions =
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private static List<string> GetNativesList(JsonElement element)
        {
            var str = element.GetRawText().Trim(['{', '}']).Split(',');
            return str.Select(s => s.Trim()).ToList();
        }

        private static List<string> GetCLassifiersList(JsonElement element)
        {
            string input = element.GetRawText();

            // 去除首尾的大括号
            string trimmedInput = input.Trim();
            trimmedInput = trimmedInput.Substring(1, trimmedInput.Length - 2).Trim();

            // 解析字符串，保留第二层大括号
            List<string> resultList = [];
            int          braceCount = 0;
            int          startIndex = 0;

            for (int i = 0; i < trimmedInput.Length; i++)
            {
                switch (trimmedInput[i])
                {
                    case '{':
                        braceCount++;
                        break;
                    case '}':
                        braceCount--;
                        break;
                    case ',' when braceCount == 0:
                        {
                            // 当 braceCount 为 0 时，表示当前逗号是第一层的分隔符
                            string item = trimmedInput.Substring(startIndex, i - startIndex).Trim();
                            resultList.Add(item);
                            startIndex = i + 1;
                            break;
                        }
                }
            }

            // 添加最后一个元素
            if (startIndex < trimmedInput.Length)
            {
                string lastItem = trimmedInput.Substring(startIndex).Trim();
                resultList.Add(lastItem);
            }

            return resultList;
        }

        private static LibrariesDownloads DowloadParser(JsonElement element)
        {
            var downloads = element.GetProperty("downloads");
            var name      = element.GetProperty("name").GetString()!;

            var result = new LibrariesDownloads
            {
                Classifiers = [],
                Natives = [],
                Rules = null,
                Artifact = null,
                Exclude = []
            };

            // try get artifact
            if (downloads.TryGetProperty("artifact", out var artifact))
            {
                var artifactContent = artifact.GetRawText();
                result.Artifact = JsonSerializer.Deserialize<Artifact>(artifactContent, SerializerOptions)!;
            }

            // try get rules
            if (element.TryGetProperty("rules", out var rules))
            {
                result.Rules = ActionParser(rules);
            }

            // try get natives
            if (element.TryGetProperty("natives", out var natives))
            {
                foreach (var item in GetNativesList(element))
                {
                    result.Natives.Add(GetNameValueTuple(item));
                }
            }

            // try get classifiers
            if (downloads.TryGetProperty("classifiers", out JsonElement value))
            {
                foreach (var item in GetCLassifiersList(value))
                {
                    result.Classifiers.Add(GetClassifiersValueTuple(item));
                }
            }

            // try get exclude
            if (element.TryGetProperty("extract", out var extract))
            {
                var exclude = extract.GetProperty("exclude").EnumerateArray();
                foreach (var item in exclude)
                {
                    result.Exclude.Add(item.GetString()!);
                }
            }

            // set name
            result.Name = name;

            return result;
        }

        private static Libraries LibrariesParser(JsonElement element)
        {
            var array  = element.EnumerateArray();
            var result = new Libraries { Downloads = [], NameUrls = [] };

            foreach (var item in array)
            {
                if (item.TryGetProperty("downloads", out _))
                {
                    result.Downloads.Add(DowloadParser(item));
                }
                else
                {
                    result.NameUrls.Add(
                        JsonSerializer.Deserialize<LibrariesNameUrl>(item.GetRawText(), SerializerOptions)!);
                }
            }

            return result;
        }

        private static Temp RebuildJson(JsonElement element)
        {
            var root   = element.EnumerateObject();
            var result = new StringBuilder("{");
            foreach (var pro in root.Where(pro => pro.Name != "arguments" && pro.Name != "libraries"))
            {
                result.Append($"{pro},");
            }

            result.Remove(result.Length - 1, 1).Append("}");

            var str = result.ToString();
            //Console.WriteLine(str);

            return JsonSerializer.Deserialize<Temp>(str, SerializerOptions)!;
        }

        public static VersionData.VersionData Parser(string input)
        {
            var result = new VersionData.VersionData();

            var root = JsonDocument.Parse(input).RootElement;

            //var arguments = root.GetProperty("arguments")
            // try get arguments
            if (root.TryGetProperty("arguments", out var arguments))
            {
                var argumentsJson = ArgumentsParser(arguments);
                result.Arguments = argumentsJson;
            }
            else
            {
                result.Arguments = null;
            }

            var rebuild   = RebuildJson(root);
            var libraries = LibrariesParser(root.GetProperty("libraries"));

            return result with
            {
                AssetIndex = rebuild.AssetIndex,
                Assets = rebuild.Assets,
                ComplianceLevel = rebuild.ComplianceLevel,
                Downloads = rebuild.Downloads,
                Id = rebuild.Id,
                JavaVersion = rebuild.JavaVersion,
                Libraries = libraries,
                Logging = rebuild.Logging,
                MainClass = rebuild.MainClass,
                MinimunLauncherVersion = rebuild.MinimunLauncherVersion,
                ReleaseTime = rebuild.ReleaseTime,
                Time = rebuild.Time,
                Type = rebuild.Type,
                ClientVersion = rebuild.ClientVersion
            };
        }

        [GeneratedRegex("\"([^\"]+)\"")]
        private static partial Regex RulesNameRegex();

        [GeneratedRegex(@"(?<=""name"":\s*)""([^""]+)""")]
        private static partial Regex JvmOsValueRegex();

        [GeneratedRegex("\"(.*?)\":\\s*\"(.*?)\"")]
        private static partial Regex GetNameValueRegex();

        [GeneratedRegex("\"([^\"]+)\":\\s*\\{")]
        private static partial Regex GetDoubleQuateValue();

        [GeneratedRegex(@"\{[^{}]*\}")]
        private static partial Regex ClassifiersContentRegex();
    }
}

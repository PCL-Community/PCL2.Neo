using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using System.IO;
using PCL2.Neo.Models.Minecraft.McVersion.VersionData;

namespace PCL2.Neo.Models.Minecraft.McVersion.VersionData.Tests
{
    [TestClass()]
    public class VersionTests
    {
    }

    [TestClass()]
    public class MethodTest
    {
        [TestMethod()]
        public void TirmTest()
        {
            string input = @"
        {
          ""natives-linux"": {
            ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-linux.jar"",
            ""sha1"": ""d898a33b5d0a6ef3fed3a4ead506566dce6720a5"",
            ""size"": 578539,
            ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-linux.jar""
          },
          ""natives-osx"": {
            ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-osx.jar"",
            ""sha1"": ""79f5ce2fea02e77fe47a3c745219167a542121d7"",
            ""size"": 468116,
            ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-osx.jar""
          },
          ""natives-windows"": {
            ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-windows.jar"",
            ""sha1"": ""78b2a55ce4dc29c6b3ec4df8ca165eba05f9b341"",
            ""size"": 613680,
            ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.2-nightly-20140822/lwjgl-platform-2.9.2-nightly-20140822-natives-windows.jar""
          }
        }";

            // 去除首尾的大括号
            string trimmedInput = input.Trim();
            trimmedInput = trimmedInput.Substring(1, trimmedInput.Length - 2).Trim();

            // 解析字符串，保留第二层大括号
            List<string> resultList = new List<string>();
            int braceCount = 0;
            int startIndex = 0;

            for (int i = 0; i < trimmedInput.Length; i++)
            {
                if (trimmedInput[i] == '{')
                {
                    braceCount++;
                }
                else if (trimmedInput[i] == '}')
                {
                    braceCount--;
                }
                else if (trimmedInput[i] == ',' && braceCount == 0)
                {
                    // 当 braceCount 为 0 时，表示当前逗号是第一层的分隔符
                    string item = trimmedInput.Substring(startIndex, i - startIndex).Trim();
                    resultList.Add(item);
                    startIndex = i + 1;
                }
            }

            // 添加最后一个元素
            if (startIndex < trimmedInput.Length)
            {
                string lastItem = trimmedInput.Substring(startIndex).Trim();
                resultList.Add(lastItem);
            }

            // 输出结果
            foreach (var item in resultList)
            {
                Console.WriteLine(item);
                Console.WriteLine(); // 分隔每个结果
            }
        }
    }
}
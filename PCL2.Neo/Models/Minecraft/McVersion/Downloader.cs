using PCL2.Neo.Models.Minecraft.McVersion.VersionManifest;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.McVersion
{
    public class Downloader
    {
        public static async Task<VersionManifestData?> GetVersionManifest()
        {
            try
            {
                using var client = new HttpClient();
                var response = await client
                    .GetStringAsync("https://launchermeta.mojang.com/mc/game/version_manifest.json");
                return JsonSerializer.Deserialize<VersionManifestData>(response);
            }
            catch (HttpRequestException e)
            {
                // TODO: Handle Exception
                throw;
            }
        }

        public static async Task<string> GetVersionInfo(VersionInfo info)
        {
            try
            {
                using var client = new HttpClient();
                return await client
                    .GetStringAsync(info.Url);
            }
            catch (HttpRequestException e)
            {
                // TODO: Handle Exception
                throw;
            }
        }
    }
}

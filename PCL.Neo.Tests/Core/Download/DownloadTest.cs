using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using PCL.Neo.Core.Download;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Tests.Core.Download;

// strong coupling, but this is only a test, who cares?
public class ConnectionCountConcern(IHandler content, DownloadTest dt) : IConcern
{
    public ValueTask PrepareAsync() => Content.PrepareAsync();

    public async ValueTask<IResponse> HandleAsync(IRequest request)
    {
        Interlocked.Increment(ref dt.ConnectionCount);
        dt.CheckNumberOfConnections();
        // await Task.Delay(500);
        var response = await Content.HandleAsync(request);
        Interlocked.Decrement(ref dt.ConnectionCount);
        return response;
    }

    public IHandler Content { get; } = content;
}

public class CustomConcernBuilder(Func<IHandler, IConcern> concernFactory) : IConcernBuilder
{
    public IConcern Build(IHandler content) => concernFactory(content);
}

public class DownloadTest
{
    public IServerHost ApiServer;
    public Dictionary<string, byte[]> TestCases = [];
    public Dictionary<string, bool> HasLied = [];
    public long ConnectionCount;
    public const int NumberOfTestCases = 16 * 1024;
    public const int SizeOfSingleTestCase = 64 * 1024;
    public const int MaxThreads = 64;
    public const bool IsLie = true;

    public ConcurrentBag<string> Output = [];

    [SetUp]
    public async Task Init()
    {
        for (int i = 0; i < NumberOfTestCases; i++)
        {
            var buffer = new byte[SizeOfSingleTestCase];
            Random.Shared.NextBytes(buffer);
            var hasher = SHA1.Create();
            var hash = Convert.ToHexStringLower(hasher.ComputeHash(buffer));
            TestCases.Add(hash, buffer);
            if (IsLie)
            {
                if (i < NumberOfTestCases / 4) // don't lie on the first quarter of test case
                    HasLied.Add(hash, true);
                else
                    HasLied.Add(hash, Random.Shared.Next(100) != 0); // 1% of chance lying
            }
            else
                HasLied.Add(hash, true);
        }

        var service = Inline.Create()
            .Get("hash/:hash", (string hash) =>
            {
                if (HasLied[hash])
                    return new MemoryStream(TestCases[hash]);
                HasLied[hash] = true;
                var buffer = new byte[SizeOfSingleTestCase];
                Random.Shared.NextBytes(buffer);
                return new MemoryStream(buffer);
            })
            .Add(new CustomConcernBuilder(x => new ConnectionCountConcern(x, this)));
        var api = Layout.Create()
            .Add("download", service);
        ApiServer = await Host.Create()
            .Handler(api)
            .Port(8000)
            .StartAsync();
    }

    public void CheckNumberOfConnections()
    {
        var count = Interlocked.Read(ref ConnectionCount);
        var msg = count > MaxThreads ? $"{count} SERVER OVERLOADED!" : count.ToString();
        Output.Add(msg);
    }

    [Test]
    public async Task InitTest()
    {
        using var client = new HttpClient();
        var expectedHash = TestCases.FirstOrDefault().Key;
        var response = await client.GetAsync($"http://127.0.0.1:8000/download/hash/{expectedHash}");
        response.EnsureSuccessStatusCode();
        var hasher = SHA1.Create();
        var content = await response.Content.ReadAsByteArrayAsync();
        Console.WriteLine(Convert.ToHexStringLower(TestCases[expectedHash][..32]) + "...");
        Console.WriteLine(Convert.ToHexStringLower(content[..32]) + "...");
        Console.WriteLine(content.SequenceEqual(TestCases[expectedHash])
            ? "Content Identical!"
            : "Content Not Identical!");
        var actualHash = Convert.ToHexStringLower(hasher.ComputeHash(content));
        Console.WriteLine($"Expected hash: {expectedHash}");
        Console.WriteLine($"Actual hash: {actualHash}");
    }

    [Test]
    public async Task SingleDownloadTest()
    {
        if (File.Exists("/tmp/downloadtest.bin"))
            File.Delete("/tmp/downloadtest.bin");
        var expectedHash = TestCases.FirstOrDefault().Key;
        await new DownloadReceipt
        {
            SourceUrl = $"http://127.0.0.1:8000/download/hash/{expectedHash}",
            DestinationPath = "/tmp/downloadtest.bin",
            Integrity = new FileIntegrity { ExpectedSize = SizeOfSingleTestCase, Hash = expectedHash }
        }.DownloadAsync();
        Assert.That(
            (await File.ReadAllBytesAsync("/tmp/downloadtest.bin"))
            .SequenceEqual(TestCases.FirstOrDefault().Value),
            Is.True);
    }

    [Test]
    public async Task MultithreadedDownloadTest()
    {
        if (Directory.Exists("/tmp/downloadtest"))
            Directory.Delete("/tmp/downloadtest", true);
        Directory.CreateDirectory("/tmp/downloadtest");
        var downloader = new Downloader(MaxThreads);
        await downloader.Download(TestCases.Select(x => new DownloadReceipt
        {
            SourceUrl = $"http://127.0.0.1:8000/download/hash/{x.Key}",
            DestinationPath = $"/tmp/downloadtest/{x.Key}",
            Integrity = new FileIntegrity { ExpectedSize = SizeOfSingleTestCase, Hash = x.Key }
        }));
        foreach (string x in Output)
        {
            TestContext.WriteLine(x);
        }

        Assert.That(Output.All(x => !x.Contains("OVERLOADED!")), Is.True);
        foreach (var (hash, data) in TestCases)
        {
            Assert.That((await File.ReadAllBytesAsync($"/tmp/downloadtest/{hash}")).SequenceEqual(data), Is.True);
        }
    }

    [Test]
    public async Task MultithreadedProgressReportTest()
    {
        if (Directory.Exists("/tmp/downloadtest"))
            Directory.Delete("/tmp/downloadtest", true);
        Directory.CreateDirectory("/tmp/downloadtest");
        var downloader = new Downloader(MaxThreads);
        var lastProgress = 0.0;
        var progress = new Progress<double>(_ =>
        {
            var p = downloader.Progress;
            if (1.0 - p > 0.00000001 && p - lastProgress < 0.01)
                return;
            if (1.0 - p <= 0.00000001 && 1.0 - lastProgress <= 0.00000001)
                return;
            lastProgress = p;
            TestContext.Progress.WriteLine($"{p * 100:0.##}% {(double)downloader.TransferRate / 1024 / 1024:0.##}MB/s");
        });
        await downloader.Download(TestCases.Select(x => new DownloadReceipt
        {
            SourceUrl = $"http://127.0.0.1:8000/download/hash/{x.Key}",
            DestinationPath = $"/tmp/downloadtest/{x.Key}",
            Integrity = new FileIntegrity { ExpectedSize = SizeOfSingleTestCase, Hash = x.Key },
            Progress = progress
        }));
        foreach (var (hash, data) in TestCases)
        {
            Assert.That((await File.ReadAllBytesAsync($"/tmp/downloadtest/{hash}")).SequenceEqual(data), Is.True);
        }
    }

    [TearDown]
    public async Task Cleanup()
    {
        if (File.Exists("/tmp/downloadtest.bin"))
            File.Delete("/tmp/downloadtest.bin");
        if (Directory.Exists("/tmp/downloadtest"))
            Directory.Delete("/tmp/downloadtest", true);
        await ApiServer.StopAsync();
    }
}
using PCL.Neo.Core.Models.Minecraft.Java;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UsePathBase("/PCL.Proto");
app.UseCors();
app.UseDefaultFiles(); // 自动寻找 index.html
app.UseStaticFiles();

IJavaManager javaManager = new JavaManager();
await javaManager.JavaListInitAsync();

// 示例函数：可以是任意逻辑
void DoSomething(string module,string data)
{
    Console.WriteLine($"收到数据：{module} {data}，处理完成！");
}

app.MapPost("/api/do-something", (MyPayload payload) =>
{
    DoSomething(payload.module, payload.message);
    return Results.Ok(new { success = true });
});


app.MapGet("/api/javalist", () => javaManager.JavaList);

// 👉 所有未匹配的路由都返回 index.html（支持前端路由）
app.MapFallbackToFile("index.html");

app.Run();

record MyPayload(string module, string message);
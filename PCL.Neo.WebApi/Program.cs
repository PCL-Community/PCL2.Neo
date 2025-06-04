using PCL.Neo.WebApi.Services;
using PCL.Neo.WebApi.Models;
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

// 注册自定义服务
builder.Services.AddSingleton<IJavaManager, JavaManager>();
builder.Services.AddSingleton<IDoSomethingService, DoSomethingService>();

// 添加控制器
builder.Services.AddControllers();

var app = builder.Build();

app.UsePathBase("/PCL.Proto");
app.UseCors();
app.UseDefaultFiles(); // 自动寻找 index.html
app.UseStaticFiles();
// 👉 所有未匹配的路由都返回 index.html（支持前端路由）
app.MapFallbackToFile("index.html");

// Map controllers
app.MapControllers();

app.Run();
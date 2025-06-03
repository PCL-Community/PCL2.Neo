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

app.UseCors();

IJavaManager javaManager = new JavaManager();
await javaManager.JavaListInitAsync();

app.MapGet("/api/javalist", () => javaManager.JavaList);

app.Run();

using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load(".env");

// Environment variables can be accessed using Environment.GetEnvironmentVariable
var url = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://0.0.0.0:8080";
builder.WebHost.UseUrls(url);

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

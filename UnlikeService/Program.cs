using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load(".env");

builder.WebHost.UseUrls("http://localhost:8080");

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();

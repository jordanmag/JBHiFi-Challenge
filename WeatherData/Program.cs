using WeatherData;
using WeatherData.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimitManager, RateLimitManager>();
builder.Services.AddSingleton<IWeatherService, WeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}");

app.UseMiddleware<ApiKeyMiddleware>();

app.Run();

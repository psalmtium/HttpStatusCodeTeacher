using HttpStatusCodeTeacher.Services;
using HttpStatusCodeTeacher.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Azure App Service (only if PORT environment variable is set)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add Application Insights telemetry
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore null values in JSON serialization
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

        // Ensure property names respect JsonPropertyName attributes
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HTTP Status Code Teacher API",
        Version = "v1.0.0",
        Description = "An AI-powered educational API that teaches HTTP status codes with detailed explanations and examples."
    });
});

// Add memory cache for in-memory caching option
builder.Services.AddMemoryCache();

// Register HttpClient for GeminiService
builder.Services.AddHttpClient<GeminiService>();

// Register AI services
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<ClaudeService>();

// Register AI factory
builder.Services.AddScoped<AiServiceFactory>();

// Register all cache service implementations
builder.Services.AddSingleton<RedisCacheService>();
builder.Services.AddSingleton<InMemoryCacheService>();
builder.Services.AddSingleton<NoCacheService>();

// Register cache factory
builder.Services.AddSingleton<CacheServiceFactory>();

// Register the cache service based on configuration
builder.Services.AddSingleton<ICacheService>(sp =>
{
    var factory = sp.GetRequiredService<CacheServiceFactory>();
    return factory.GetCacheService();
});

// Add CORS if needed
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

// Configure the HTTP request pipeline

// Add request/response logging (should be early in the pipeline)
app.UseRequestResponseLogging();

// Enable Swagger in all environments for now (can restrict to Development later)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

// Add root endpoint redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();

using Microsoft.Extensions.AI;
using NashAI_app.Services.Ingestion;
using Project_Manassas.Database;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NashAI_app.utils;
using OpenAI;
using Npgsql;
using Pgvector;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();

// var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//
// var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
// dataSourceBuilder.UseVector();
// var dataSource = dataSourceBuilder.Build();
//
// builder.Services.AddSingleton(dataSource);

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

// Add FusionCach
builder.Services.AddFusionCache()
    .WithDefaultEntryOptions(new FusionCacheEntryOptions
    {
        Duration = TimeSpan.FromHours(1),
        Priority = CacheItemPriority.High,
        FailSafeThrottleDuration = TimeSpan.FromMinutes(1),
    });

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        opt.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Register IChatClient for OpenAI
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                 ?? builder.Configuration["OPENAI_API_KEY"]
                 ?? throw new Exception("OPENAI_API_KEY environment variable not found");

    var openAICLient = new OpenAI.Chat.ChatClient(
        model: "gpt-4o-mini",
        apiKey: apiKey).AsIChatClient();
  
    return openAICLient;
});


builder.Services.AddApplicationServices();


// --- JWT Authentication (example) ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false;
    opt.SaveToken = true;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings?.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
    };
});

// --- Database: Neon/Postgres with pgvector ---
var neonConnection = Environment.GetEnvironmentVariable("NEON_API_KEY");
builder.Services.AddDbContext<ProjectContext>(options =>
{
    options.UseNpgsql(neonConnection, npgsql => npgsql.UseVector());
});

// --- OpenAI Client ---
builder.Services.AddSingleton<OpenAIClient>(sp =>
{
    var apiKey = builder.Configuration["OPENAI_API_KEY"]
                 ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("OpenAI API key not configured.");

    return new OpenAIClient(apiKey);
});

// --- Microsoft.Extensions.AI OpenAI Embedding Generator ---
builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(sp =>
{
    var client = sp.GetRequiredService<OpenAIClient>();
    // Use preview extension method from Microsoft.Extensions.AI.OpenAI
    return client.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();
});

// --- Data Ingestion Service ---
builder.Services.AddScoped<DataIngestorVB>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Build app
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowReactDev");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Example: ingest PDFs from /wwwroot/Data at startup
using (var scope = app.Services.CreateScope())
{
    var ingestor = scope.ServiceProvider.GetRequiredService<DataIngestorVB>();
    await ingestor.IngestPdfsAsync(Path.Combine(builder.Environment.WebRootPath, "Data"));
}

app.MapGet("/", () => "Hello User!");
app.Run();

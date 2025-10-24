using Microsoft.Extensions.AI;
using NashAI_app.Services;
using NashAI_app.Services.Ingestion;
using OpenAI;
using System.ClientModel;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NashAI_app.utils;
using OpenAI.Chat;
using Project_Manassas.Database;

var builder = WebApplication.CreateBuilder(args);

// Load .Env before configuration is built
Env.Load();

// Add Environmental variables
builder.Configuration.AddEnvironmentVariables();

// Add CORS services to container for REACT frontend to call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev",
        policy => policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

// Add Service
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.IgnoreNullValues = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddApplicationServices();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT settings
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(option =>
    {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(option =>
    {
        option.RequireHttpsMetadata = false;
        option.SaveToken = true;
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings?.Issuer,
        
            ValidateAudience = true,
            ValidAudience = jwtSettings?.Audience,
        
            ValidateLifetime = true,
        
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey))
            // Tells ASP.NET Core ow to validate incoming JWT tokens
        
        };
    });

builder.Services.AddControllersWithViews();

// Setting up Neon database
var neonAPIKey = Environment.GetEnvironmentVariable("NEON_API_KEY");

builder.Services.AddDbContext<ProjectContext>(options =>
{
    options.UseNpgsql(neonAPIKey);
});

// Not using Razor Pages
//builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Setting up secret keys
/*
 * dotnet user-secrets init
 * dotnet user-secrets set "OpenAI:ApiKey" "yourapikey"
 */

// Setting up OpenAI API
var credential = new ApiKeyCredential(builder.Configuration["OpenAI:ApiKey"]);
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://api.openai.com/v1")
};

// Chatting and Embeddings
var ghModelsClient = new OpenAIClient(credential, openAIOptions);
var chatClient = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
var embeddingGenerator = ghModelsClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

// Adding SqlLite Vectorpath
var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteCollection<string, IngestedChunk>("data-nashai_app-chunks", vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedDocument>("data-nashai_app-documents", vectorStoreConnectionString);

// Open Sqlite
using var connection = new SqliteConnection($"Data Source={vectorStorePath}");
connection.Open();

var command = connection.CreateCommand();
command.CommandText ="SELECT * FROM [data-nashai_app-chunks] LIMIT 5;";

using var reader = command.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine(reader.GetString(0));
}

// Data Ingestion Service
builder.Services.AddScoped<DataIngestor>();

// Semantic Search
builder.Services.AddSingleton<SemanticSearch>();

// Add Chat Client
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();

// Add Embedding Generation
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

// Build the App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("AllowReactDev");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// app.Urls.Add($"http://*:{port}");

app.MapGet("/", () => "Hello User!");

app.Run();

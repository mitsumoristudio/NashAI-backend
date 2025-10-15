using Microsoft.Extensions.AI;
using NashAI_app.Components;
using NashAI_app.Services;
using NashAI_app.Services.Ingestion;
using OpenAI;
using System.ClientModel;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .Env before configuration is built
Env.Load();

// Add Environmental variables
builder.Configuration.AddEnvironmentVariables();

// Add Service
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



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

// Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment())
// {
//     app.UseExceptionHandler("/Error", createScopeForErrors: true);
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Razor Pages dependencies
// app.UseAntiforgery();

// app.UseStaticFiles();
//app.MapRazorComponents<App>()
//    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.MapGet("/", () => "Hello World!");

app.Run();

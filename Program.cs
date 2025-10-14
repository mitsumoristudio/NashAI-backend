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

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Setting up secret keys
/*
 * dotnet user-secrets init
 * dotnet user-secrets set "OpenAI:ApiKey" "yourapikey"
 */

var credential = new ApiKeyCredential(builder.Configuration["OpenAI:ApiKey"]);
var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://api.openai.com/v1")
};

var ghModelsClient = new OpenAIClient(credential, openAIOptions);
var chatClient = ghModelsClient.GetChatClient("gpt-4o-mini").AsIChatClient();
var embeddingGenerator = ghModelsClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteCollection<string, IngestedChunk>("data-nashai_app-chunks", vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedDocument>("data-nashai_app-documents", vectorStoreConnectionString);

builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, we ingest PDF files from the /wwwroot/Data directory. You can ingest from
// other sources by implementing IIngestionSource.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.
await DataIngestor.IngestDataAsync(
    app.Services,
    new PDFDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

app.Run();

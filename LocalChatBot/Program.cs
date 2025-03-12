using LocalChatBot.Components;
using LocalChatBot.Services;
using LocalChatBot.Services.Ingestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434"),
    "gemma3");
var ollama = new OllamaApiClient(new Uri("http://localhost:11434"),
    "all-minilm");

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = ollama;

var vectorStore = new JsonVectorStore(Path.Combine(AppContext.BaseDirectory, "vector-store"));

builder.Services.AddSingleton<IVectorStore>(vectorStore);
builder.Services.AddScoped<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

builder.Services.AddDbContext<IngestionCacheDbContext>(options =>
    options.UseSqlite("Data Source=ingestioncache.db"));

var app = builder.Build();
IngestionCacheDbContext.Initialize(app.Services);

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
    new PdfDirectorySource(Path.Combine(builder.Environment.WebRootPath, "Data")));

await app.RunAsync();

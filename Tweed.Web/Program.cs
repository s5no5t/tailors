using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Tweed.Data;
using Tweed.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var store = new DocumentStore
{
    Urls = new[] { "http://localhost:8080" },
    Database = "Tweed"
};
store.Initialize();

builder.Services.AddSingleton<IDocumentStore>(store);

builder.Services.AddScoped<IAsyncDocumentSession>(serviceProvider => serviceProvider
    .GetService<IDocumentStore>()
    ?.OpenAsyncSession() ?? throw new InvalidOperationException());

builder.Services.AddScoped<ITweedQueries, TweedQueries>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) Setup.EnsureDatabaseExists(store);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.UseMiddleware<SaveChangesMiddleware>();

app.Run();

using Microsoft.AspNetCore.Identity;
using Raven.Client.Documents;
using Raven.Client.NodaTime;
using Raven.DependencyInjection;
using Raven.Identity;
using Tweed.Data;
using Tweed.Web;
using Tweed.Web.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddRavenDbDocStore(options =>
{
    options.BeforeInitializeDocStore = store => { store.ConfigureForNodaTime(); };
});
builder.Services.AddRavenDbAsyncSession();

var identityBuilder = builder.Services
    .AddDefaultIdentity<AppUser>()
    .AddRavenDbIdentityStores<AppUser>();

identityBuilder.AddDefaultUI();

builder.Services.AddScoped<ITweedQueries, TweedQueries>();

builder.Services.AddRazorPages()
    .AddMvcOptions(o => o.Filters.Add<RavenSaveChangesAsyncFilter>());

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var documentStore = app.Services.GetRequiredService<IDocumentStore>();
    documentStore.EnsureDatabaseExists();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

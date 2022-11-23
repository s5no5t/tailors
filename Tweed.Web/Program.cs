using Microsoft.AspNetCore.Identity;
using Raven.Identity;
using Tweed.Data;
using Tweed.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var ravenDbStore = new RavenDbStore();
builder.Services.AddSingleton(ravenDbStore);

builder.Services.AddScoped(serviceProvider => serviceProvider
    .GetService<RavenDbStore>()
    ?.OpenSession() ?? throw new InvalidOperationException());

var identityBuilder = builder.Services
    .AddDefaultIdentity<AppUser>()
    .AddRavenDbIdentityStores<AppUser>();

identityBuilder.AddDefaultUI();

builder.Services.AddScoped<ITweedQueries, TweedQueries>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) ravenDbStore.EnsureDatabaseExists();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.UseMiddleware<SaveChangesMiddleware>();

app.Run();

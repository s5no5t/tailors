using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Trace;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Raven.Identity;
using Tweed.Data;
using Tweed.Data.Model;
using Tweed.Web.Areas.Identity;
using Tweed.Web.Filters;
using Tweed.Web.Helper;
using IdentityRole = Raven.Identity.IdentityRole;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<RavenSaveChangesAsyncPageFilter>());

builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());

builder.Services.AddRavenDbDocStore(options =>
{
    options.BeforeInitializeDocStore = store => store.PreInitialize();
});
builder.Services.AddRavenDbAsyncSession();

builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    })
    .AddRavenDbIdentityStores<AppUser,
        IdentityRole>(
        _ => // empty options is a workaround for an exception in case this param is null
        {
        })
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(
    options => options.LoginPath = "/Identity/Account/login");

builder.Services.AddScoped<ITweedQueries, TweedQueries>();
builder.Services.AddScoped<INotificationManager, NotificationManager>();
builder.Services.AddScoped<IAppUserQueries, AppUserQueries>();
builder.Services.AddScoped<IAppUserFollowsQueries, AppUserFollowsQueries>();
builder.Services.AddScoped<IAppUserLikesQueries, AppUserLikesQueries>();
builder.Services.AddScoped<IFeedBuilder, FeedBuilder>();
builder.Services.AddScoped<IViewModelFactory, ViewModelFactory>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();

var honeycombOptions = builder.Configuration.GetHoneycombOptions();

// Setup OpenTelemetry Tracing
builder.Services.AddOpenTelemetry().WithTracing(otelBuilder =>
{
    otelBuilder
        .AddHoneycomb(honeycombOptions)
        .AddCommonInstrumentations();
});

var app = builder.Build();

app.Lifetime.ApplicationStarted.Register(() =>
{
    var documentStore = app.Services.GetRequiredService<IDocumentStore>();
    documentStore.EnsureDatabaseExists();
    documentStore.DeployIndexes();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

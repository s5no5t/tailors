using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using OpenTelemetry.Trace;
using Raven.DependencyInjection;
using Raven.Identity;
using Tweed.Like.Domain;
using Tweed.Like.Infrastructure;
using Tweed.Thread.Domain;
using Tweed.Thread.Infrastructure;
using Tweed.User.Domain;
using Tweed.User.Infrastructure;
using Tweed.Web;
using Tweed.Web.Areas.Identity;
using Tweed.Web.Filters;
using Tweed.Web.Helper;
using IdentityRole = Raven.Identity.IdentityRole;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddMvcOptions(options => options.Filters.Add<RavenSaveChangesAsyncPageFilter>());
builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new FeatureFolderLocationExpander());
});

builder.Services.AddHttpContextAccessor();

SetupRavenDbServices(builder);
SetupIdentity(builder);
SetupOpenTelemetry(builder);
SetupAssemblyScanning(builder);

builder.Services.AddHostedService<TweedThreadUpdateSubscriptionWorker>();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Feed}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

static void SetupRavenDbServices(WebApplicationBuilder builder)
{
    builder.Services.AddRavenDbDocStore(options =>
    {
        options.BeforeInitializeDocStore = store =>
        {
            store.PreInitialize();
            store.PreInitializeLikes();
        };
        options.AfterInitializeDocStore = store =>
        {
            store.EnsureDatabaseExists();
            store.DeployUserIndexes();
            store.DeployTweedIndexes();
        };
    });
    builder.Services.AddRavenDbAsyncSession();
}

static void SetupIdentity(WebApplicationBuilder builder)
{
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

    
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;
    });
}

static void SetupOpenTelemetry(WebApplicationBuilder builder)
{
    var honeycombOptions = builder.Configuration.GetHoneycombOptions();
    builder.Services.AddOpenTelemetry().WithTracing(otelBuilder =>
    {
        otelBuilder
            .AddHoneycomb(honeycombOptions)
            .AddAspNetCoreInstrumentationWithBaggage()
            .AddCommonInstrumentations();
    });
}

static void SetupAssemblyScanning(WebApplicationBuilder builder)
{
    builder.Services.Scan(scan =>
    {
        scan.FromCallingAssembly().AddClasses().AsMatchingInterface();
        scan.FromAssembliesOf(typeof(UserRepository)).AddClasses().AsMatchingInterface();
        scan.FromAssembliesOf(typeof(AppUser)).AddClasses().AsMatchingInterface();
        scan.FromAssembliesOf(typeof(ShowFeedUseCase)).AddClasses().AsMatchingInterface();
        scan.FromAssembliesOf(typeof(UserLikes)).AddClasses().AsMatchingInterface();
        scan.FromAssembliesOf(typeof(Tweed.Thread.Domain.Tweed)).AddClasses().AsMatchingInterface();
    });
}

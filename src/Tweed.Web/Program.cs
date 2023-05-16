using Microsoft.AspNetCore.Identity;
using OpenTelemetry.Trace;
using Raven.DependencyInjection;
using Raven.Identity;
using Tweed.Feed.Domain;
using Tweed.Like.Domain;
using Tweed.Like.Infrastructure;
using Tweed.Thread.Domain;
using Tweed.Thread.Infrastructure;
using Tweed.User.Domain;
using Tweed.User.Infrastructure;
using Tweed.Web;
using Tweed.Web.Areas.Identity;
using Tweed.Web.Filters;
using IdentityRole = Raven.Identity.IdentityRole;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<RavenSaveChangesAsyncPageFilter>());

builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());

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

builder.Services.Scan(scan =>
{
    scan.FromCallingAssembly().AddClasses().AsMatchingInterface();
    scan.FromAssembliesOf(typeof(UserRepository)).AddClasses().AsMatchingInterface();
    scan.FromAssembliesOf(typeof(AppUser)).AddClasses().AsMatchingInterface();
    scan.FromAssembliesOf(typeof(ShowFeedUseCase)).AddClasses().AsMatchingInterface();
    scan.FromAssembliesOf(typeof(UserLikes)).AddClasses().AsMatchingInterface();
    scan.FromAssembliesOf(typeof(Tweed.Thread.Domain.Tweed)).AddClasses().AsMatchingInterface();
});

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
        .AddAspNetCoreInstrumentationWithBaggage()
        .AddCommonInstrumentations();
});

builder.Services.AddHostedService<TweedThreadUpdateSubscriptionWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Feed}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
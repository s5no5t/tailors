using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using OpenTelemetry.Trace;
using Raven.DependencyInjection;
using Raven.Identity;
using Tailors.Domain.ThreadAggregate;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Infrastructure;
using Tailors.Infrastructure.ThreadAggregate;
using Tailors.Infrastructure.TweedAggregate;
using Tailors.Infrastructure.UserAggregate;
using Tailors.Infrastructure.UserFollowsAggregate;
using Tailors.Infrastructure.UserLikesAggregate;
using Tailors.Web.Areas.Identity;
using Tailors.Web.Filters;
using Tailors.Web.Helper;
using IdentityRole = Raven.Identity.IdentityRole;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add<RavenSaveChangesAsyncPageFilter>());
builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new FeatureFolderLocationExpander());
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

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
        options.BeforeInitializeDocStore = store => { store.PreInitialize(); };
        options.AfterInitializeDocStore = store =>
        {
            store.EnsureDatabaseExists();
            store.DeployIndexes();
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
        options =>
        {
            options.LoginPath = "/Identity/Account/login";
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    const string hxRedirectHeader = "Hx-Redirect";
                    const string hxRequestHeader = "Hx-Request";
                    const string hxBoostedHeader = "Hx-Boosted";

                    if (IsHtmxRequest(context.Request))
                    {
                        context.Response.StatusCode = 401;
                        if (IsHtmxBoostedRequest(context.Request))
                            context.Response.Headers[hxRedirectHeader] = context.RedirectUri;
                    }
                    else
                    {
                        context.Response.Redirect(context.RedirectUri);
                    }

                    return Task.CompletedTask;

                    static bool IsHtmxRequest(HttpRequest request)
                    {
                        return string.Equals(request.Headers[hxRequestHeader], "true",
                            StringComparison.Ordinal);
                    }

                    static bool IsHtmxBoostedRequest(HttpRequest request)
                    {
                        return string.Equals(request.Headers[hxBoostedHeader], "true");
                    }
                }
            };
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
    builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
    builder.Services.AddScoped<ITweedRepository, TweedRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserFollowsRepository, UserFollowsRepository>();
    builder.Services.AddScoped<IUserLikesRepository, UserLikesRepository>();
    builder.Services.AddScoped<INotificationManager, NotificationManager>();
    builder.Services.AddScoped<FollowUserUseCase>();
    builder.Services.AddScoped<ShowFeedUseCase>();
    builder.Services.AddScoped<ThreadUseCase>();
    builder.Services.AddScoped<CreateTweedUseCase>();
    builder.Services.AddScoped<LikeTweedUseCase>();
    builder.Services.AddScoped<TweedViewModelFactory>();
}

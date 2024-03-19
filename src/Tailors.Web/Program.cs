using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using Octokit;
using OpenTelemetry.Trace;
using Raven.Client.Documents;
using Raven.DependencyInjection;
using Tailors.Domain.TweedAggregate;
using Tailors.Domain.UserAggregate;
using Tailors.Domain.UserFollowsAggregate;
using Tailors.Domain.UserLikesAggregate;
using Tailors.Infrastructure;
using Tailors.Infrastructure.TweedAggregate;
using Tailors.Infrastructure.UserAggregate;
using Tailors.Infrastructure.UserFollowsAggregate;
using Tailors.Infrastructure.UserLikesAggregate;
using Tailors.Web.Features.Tweed;
using Tailors.Web.Filters;
using Tailors.Web.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new FeatureFolderLocationExpander());
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Work around issue with X-Forwarded-For header forwarding
    // Should be the fly.io networks, but it's not clear which addresses these are
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/signin";
        options.LogoutPath = "/signout";
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["GitHub:ClientId"] ??
                           throw new ArgumentException("GitHub:ClientId is missing");
        options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ??
                               throw new ArgumentException("GitHub:ClientSecret is missing");

        options.SaveTokens = true;

        options.Events.OnCreatingTicket = async context =>
        {
            var githubUsername = context.User.GetProperty("login").GetString();
            if (githubUsername is null) throw new InvalidOperationException("username is null");

            var gitHubClient = new GitHubClient(new ProductHeaderValue("tailors"))
            {
                Credentials = new Credentials(context.AccessToken)
            };

            var emails = await gitHubClient.User.Email.GetAll();
            var primaryEmail = emails.FirstOrDefault(e => e.Primary);
            if (primaryEmail is null) throw new InvalidOperationException("No primary email found");

            var user = await FindOrCreateAppUser(context, primaryEmail.Email, githubUsername);

            context.Identity!.AddClaim(new Claim(ClaimsPrincipalExtensions.UrnTailorsAppUserId, user.Id!));
        };
    });

SetupRavenDbServices(builder);
SetupOpenTelemetry(builder);
SetupAssemblyScanning(builder);

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
    builder.Services.AddScoped<AuthenticationUseCase>();
    builder.Services.AddScoped<TweedViewModelFactory>();
}

async Task<AppUser> FindOrCreateAppUser(OAuthCreatingTicketContext oAuthCreatingTicketContext, string email,
    string userName)
{
    var session = oAuthCreatingTicketContext.HttpContext.RequestServices.GetRequiredService<IDocumentStore>();
    using var asyncSession = session.OpenAsyncSession();
    IUserRepository userRepository = new UserRepository(asyncSession);
    var authenticationUseCase = new AuthenticationUseCase(userRepository);
    var appUser = await authenticationUseCase.EnsureUserExists(email, userName);
    await asyncSession.SaveChangesAsync();
    return appUser;
}

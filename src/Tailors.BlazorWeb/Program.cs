using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Raven.DependencyInjection;
using Raven.Identity;
using Tailors.BlazorWeb.Components;
using Tailors.BlazorWeb.Components.Account;
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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<AppUser>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddSignInManager()
    .AddDefaultTokenProviders()
    .AddRavenDbIdentityStores<AppUser>();

builder.Services.AddSingleton<IEmailSender<AppUser>, IdentityNoOpEmailSender>();

SetupRavenDbServices(builder);
SetupAssemblyScanning(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

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

static void SetupAssemblyScanning(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
    builder.Services.AddScoped<ITweedRepository, TweedRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserFollowsRepository, UserFollowsRepository>();
    builder.Services.AddScoped<IUserLikesRepository, UserLikesRepository>();
    //builder.Services.AddScoped<INotificationManager, NotificationManager>();
    builder.Services.AddScoped<FollowUserUseCase>();
    builder.Services.AddScoped<ShowFeedUseCase>();
    builder.Services.AddScoped<ThreadUseCase>();
    builder.Services.AddScoped<CreateTweedUseCase>();
    builder.Services.AddScoped<LikeTweedUseCase>();
    //builder.Services.AddScoped<TweedViewModelFactory>();
}

using Raven.Client.Documents;
using Raven.Client.NodaTime;
using Raven.DependencyInjection;
using Raven.Identity;
using Tweed.Data;
using Tweed.Data.Entities;
using Tweed.Web;
using Tweed.Web.Areas.Identity;
using Tweed.Web.Filters;
using Tweed.Web.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(o => o.Filters.Add<RavenSaveChangesAsyncActionFilter>());

builder.Services.AddRavenDbDocStore(options =>
{
    options.BeforeInitializeDocStore = store => { store.ConfigureForNodaTime(); };
});
builder.Services.AddRavenDbAsyncSession();

builder.Services.AddScoped<IEmailSender, EmailSender>();

var identityBuilder = builder.Services
    .AddIdentity<AppUser, IdentityRole>()
    .AddRavenDbIdentityStores<AppUser, IdentityRole>();

builder.Services.ConfigureApplicationCookie(
    options => options.LoginPath = "/Identity/Account/login");

builder.Services.AddScoped<ITweedQueries, TweedQueries>();
builder.Services.AddScoped<INotificationManager, NotificationManager>();
builder.Services.AddScoped<IAppUserQueries, AppUserQueries>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddRazorPages();

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

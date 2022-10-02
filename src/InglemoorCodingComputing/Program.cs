global using System.ComponentModel.DataAnnotations;
global using System.Text;
global using Azure.Storage.Blobs;
global using InglemoorCodingComputing.Models;
global using InglemoorCodingComputing.Services;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Azure.Cosmos;

using System.Globalization;

CultureInfo.CurrentCulture = new("en-US");

var builder = WebApplication.CreateBuilder(args);

async Task<CosmosClient> ConfigureCosmos(IConfigurationSection config)
{
    CosmosClient cosmos = new(config["ConnStr"], new() { SerializerOptions = new() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase } });
    var throughput = ThroughputProperties.CreateManualThroughput(int.Parse(config["Throughput"]));
    var db = (await cosmos.CreateDatabaseIfNotExistsAsync(config["DatabaseName"], throughput)).Database;
    _ = await db.CreateContainerIfNotExistsAsync(new(config["AuthContainer"], "/id"));
    _ = await db.CreateContainerIfNotExistsAsync(new(config["UserContainer"], "/id"));
    _ = await db.CreateContainerIfNotExistsAsync(new(config["MeetingsContainer"], "/id"));
    _ = await db.CreateContainerIfNotExistsAsync(new(config["StaticPagesContainer"], "/id"));
    _ = await db.CreateContainerIfNotExistsAsync(new(config["ShortenedURLContainer"], "/shortened"));
    _ = await db.CreateContainerIfNotExistsAsync(new(config["ApprovedEmailsContainer"], "/id"));
    return cosmos;
}

BlobServiceClient ConfigureBlob(IConfigurationSection config)
{
    BlobServiceClient blobClient = new(config["ConnStr"]);
    string[] clients = { "static" };
    foreach (var client in clients)
        blobClient.GetBlobContainerClient(client).CreateIfNotExists();
    return blobClient;
}

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddLocalization();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddGoogle(gOptions =>
    {
        gOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        gOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton(x => ConfigureCosmos(x.GetService<IConfiguration>()!.GetSection("Cosmos")).Result);
builder.Services.AddSingleton(x => ConfigureBlob(x.GetService<IConfiguration>()!.GetSection("BlobStorage")));
builder.Services.AddSingleton<IUserAuthService, UserAuthService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IStaticResourceService, StaticResourceService>();
builder.Services.AddSingleton<IApprovedEmailsService, ApprovedEmailsService>();
builder.Services.AddSingleton<IURLShortenerService, URLShortenerService>();
builder.Services.AddSingleton<ICacheEventService, CacheEventService>();
builder.Services.AddSingleton<UserLogoutManager>();
builder.Services.AddSingleton<URLShortenerEndpointDataSource>();
builder.Services.AddSingleton(_ =>
{
    Ganss.XSS.HtmlSanitizer x = new();
    x.AllowedAttributes.Add("class");
    x.AllowedAttributes.Add("id");
    return x;
});
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IUserStateService, UserStateService>();
builder.Services.AddScoped<IMeetingsService, MeetingsService>();
builder.Services.AddScoped<IStaticPageService, StaticPageService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped<CustomCookieAuthenticationEvents>();

builder.Services.AddTransient<IRouteAnalyzerService, RouteAnalyzerService>();
builder.Services.AddTransient(typeof(ICacheService<>), typeof(PersistentCacheService<>));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

var app = builder.Build();

// prewarm services
app.Services.GetService<IUserAuthService>(); // blazor has hard time finding this service injected into a razor compoenent when it hasn't been requested first outside of blazor.

app.UseRequestLocalization("en-US");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapRazorPages();
app.MapControllers();

((IEndpointRouteBuilder)app).DataSources.Add(app.Services.GetService<URLShortenerEndpointDataSource>() ?? throw new ArgumentNullException());

app.MapFallbackToPage("/_Host");

app.Run();

global using System.ComponentModel.DataAnnotations;
global using System.Text;
global using Azure.Storage.Blobs;
global using InglemoorCodingComputing.Models;
global using InglemoorCodingComputing.Services;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Azure.Cosmos;

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
builder.Services.AddSingleton<UserLogoutManager>();
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

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

var app = builder.Build();

// prewarm services
app.Services.GetService<IUserAuthService>(); // blazor has hard time finding this service injected into a razor compoenent when it hasn't been requested first outside of blazor.

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
app.UseAuthentication();

app.MapControllers();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Extensions.Azure;
using BlobApplication.Models;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Authentication and authorization
var initialScopes = builder.Configuration["DownstreamApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');
// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["storage-connection:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["storage-connection:queue"], preferMsi: true);
});

// Add appsettings.json to load connection data
builder.Configuration.AddJsonFile("appsettings.json");

// Add Azure Blob Storage and CosmosClient configuration to services
builder.Services.AddScoped<IBlobModel>(service =>
{
    var configuration = service.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetValue<string>("BlobService:ConnectionString");
    var containerName = configuration.GetValue<string>("BlobService:ContainerName");
    CosmosClient cosmosClient = new CosmosClient(builder.Configuration["CosmosDb:ConnectionStrings:cosmosdb-logs"]);
    return new BlobModel(connectionString, containerName, cosmosClient);
});

// Add MVC and other services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapControllers();
app.Run();

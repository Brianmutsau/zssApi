using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")!));

// IHttpClientFactory + named client "PurchaseApi" (safe defaults)
builder.Services.AddHttpClient("PurchaseApi", client =>
{
    // Ensure trailing slash so relative paths combine correctly
    var baseUrl = builder.Configuration["PurchaseApi:BaseUrl"] ?? "https://secure.v.co.zw/interview/";
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl);

    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// MVC + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WebApplication1 API",
        Version = "v1",
        Description = "ZSS Books & Categories API"
    });
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 API v1");
});

app.UseHttpsRedirection();
app.MapControllers();
app.Run();



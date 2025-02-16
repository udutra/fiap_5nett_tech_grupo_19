using System.Reflection;
using fiap_5nett_tech.Api.ReadContact.Services;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Application.Service;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Infrastructure.Data;
using fiap_5nett_tech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction => sqlServerOptionsAction.EnableRetryOnFailure())
    , ServiceLifetime.Scoped);

builder.Services.UseHttpClientMetrics();
builder.Services.AddOpenTelemetry()
    .WithMetrics(b =>
    {
        b.AddPrometheusExporter();
        b.AddAspNetCoreInstrumentation();
        b.AddRuntimeInstrumentation();
        b.AddHttpClientInstrumentation();
        // Metrics provider from OpenTelemetry
        b.AddAspNetCoreInstrumentation();
        //.AddMeter(greeterMeter.Name)
        // Metrics provides by ASP.NET Core in .NET 8
        b.AddMeter("Microsoft.AspNetCore.Hosting");
        b.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tech Challenge 1", Version = "" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost.com",
                    "http://170.0.0.1")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IContactInterface, ContactService>();
builder.Services.AddHostedService<RabbitMqReadContactGetOneByIdConsumerCs>();
builder.Services.AddHostedService<RabbitMqReadContactGetOneByDddAndPhoneConsumerCs>();
builder.Services.AddHostedService<RabbitMqReadContactGetAllConsumerCs>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

try{
    dbContext.Database.Migrate();// Aplica as Migrations
}
catch (Exception ex)
{
    Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
}

//Prometheus
var counter = Metrics.CreateCounter("webapimetricRead", "count requests to the Web Api Read Endpoint",
    new CounterConfiguration()
    {
        LabelNames = ["method", "endpoint"]
    });

app.Use((context, next) =>
{
    counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
    return next();
});

app.UseMetricServer();
app.UseHttpMetrics(options =>
{
    options.AddRouteParameter("route"); // Adiciona o rótulo "route"
});

app.MapPrometheusScrapingEndpoint();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

app.UseSwagger();
app.UseSwaggerUI();


app.Run();

/// <summary>
/// 
/// </summary>
public partial class Program { }
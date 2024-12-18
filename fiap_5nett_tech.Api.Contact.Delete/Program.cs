using System.Reflection;
using fiap_5nett_tech.Api.Contact.Delete.Services;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Application.Service;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Infrastructure.Data;
using fiap_5nett_tech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;

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

builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<IContactInterface, ContactService>();
builder.Services.AddHostedService<RabbitMqDeleteContactConsumerCs>();

var app = builder.Build();


//Prometheus
var counter = Metrics.CreateCounter("webapimetricDelete", "count requests to the Web Api Delete Endpoint",
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
app.UseHttpMetrics();
app.MapPrometheusScrapingEndpoint();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Hello World!");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public partial class Program { }
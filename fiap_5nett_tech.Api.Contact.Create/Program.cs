using System.Reflection;
using fiap_5nett_tech.Api.Contact.Create.Services;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Application.Service;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Infrastructure.Data;
using fiap_5nett_tech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlServerOptionsAction => sqlServerOptionsAction.EnableRetryOnFailure())
    , ServiceLifetime.Scoped);

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
builder.Services.AddHostedService<RabbitMqAddContactConsumerCs>();

var app = builder.Build();

//Prometheus
var counter = Metrics.CreateCounter("webapimetricCreate", "count requests to the Web Api Create Endpoint",
    new CounterConfiguration()
    {
        LabelNames = new[] { "method", "endpoint" }
    });

app.Use((context, next) =>
{
    counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
    return next();
});

app.UseMetricServer();
app.UseHttpMetrics();

app.UseHttpsRedirection();
app.UseStaticFiles();
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
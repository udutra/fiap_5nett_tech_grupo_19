using System.Reflection;
using fiap_5nett_tech.api.Consumers;
using fiap_5nett_tech.api.Interfaces;
using fiap_5nett_tech.api.Services;
using fiap_5nett_tech.Application.Interface;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Infrastructure.Data;
using fiap_5nett_tech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Registre o DbContext com a string de conex�o do banco
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Registre os servi�os e reposit�rios com o ciclo de vida Scoped
builder.Services.AddScoped<IContactInterface, ContactService>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();

// Registre o consumidor RabbitMQ
builder.Services.AddScoped<RabbitMqAddUserConsumer>();

// UseHttpClientMetrics � uma configura��o para monitoramento HTTP
builder.Services.UseHttpClientMetrics();

// Registre os controllers e o Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tech Challenge 1", Version = "" });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Registre o RabbitMqAddUserConsumerService como um HostedService
builder.Services.AddHostedService<RabbitMqAddUserConsumerService>();

// Configure a instrumenta��o do OpenTelemetry para m�tricas
builder.Services.AddOpenTelemetry()
    .WithMetrics(b =>
    {
        b.AddPrometheusExporter();
        b.AddAspNetCoreInstrumentation();
        b.AddRuntimeInstrumentation();
        b.AddHttpClientInstrumentation();
    });

var app = builder.Build();

// Use o servidor de m�tricas Prometheus
app.UseMetricServer();
app.UseHttpMetrics();
app.MapPrometheusScrapingEndpoint();

// Configura��es padr�o para a aplica��o
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Hello World!");

// Configura��o Swagger apenas no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

public partial class Program { }

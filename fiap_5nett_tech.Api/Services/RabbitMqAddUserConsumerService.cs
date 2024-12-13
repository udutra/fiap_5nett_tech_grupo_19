using fiap_5nett_tech.api.Consumers;
using fiap_5nett_tech.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace fiap_5nett_tech.api.Services;

public class RabbitMqAddUserConsumerService : IHostedService
{
    private RabbitMqAddUserConsumer _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RabbitMqAddUserConsumerService> _logger;
    private IServiceScope _serviceScope; // Manter o escopo ativo durante a execução

    public RabbitMqAddUserConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RabbitMqAddUserConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public RabbitMqAddUserConsumer GetConsumer()
    {
        return _consumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Criar um escopo persistente para o tempo de vida do serviço
            _serviceScope = _serviceScopeFactory.CreateScope();

            // Resolver dependências do consumidor
            var contactRepository = _serviceScope.ServiceProvider.GetRequiredService<IContactRepository>();
            var regionRepository = _serviceScope.ServiceProvider.GetRequiredService<IRegionRepository>();

            // Criar e iniciar o consumidor
            _consumer = new RabbitMqAddUserConsumer(contactRepository, regionRepository);
            _consumer.Start(); // Inicia o consumidor

            _logger.LogInformation("RabbitMqAddUserConsumer iniciado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar o RabbitMqAddUserConsumer.");
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Parar o consumidor, se necessário
            _consumer?.Stop();

            // Liberar o escopo de serviço
            _serviceScope?.Dispose();

            _logger.LogInformation("RabbitMqAddUserConsumer parado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar o RabbitMqAddUserConsumer.");
        }

        return Task.CompletedTask;
    }
}

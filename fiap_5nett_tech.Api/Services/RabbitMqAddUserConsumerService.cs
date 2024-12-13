using fiap_5nett_tech.api.Consumers;
using fiap_5nett_tech.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace fiap_5nett_tech.api.Services;

public class RabbitMqAddUserConsumerService : IHostedService
{
    private RabbitMqAddUserConsumer _consumer;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<RabbitMqAddUserConsumerService> _logger;

    // Construtor com injeção de dependência para IServiceScopeFactory e ILogger
    public RabbitMqAddUserConsumerService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<RabbitMqAddUserConsumerService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    // Método para acessar o consumidor
    public RabbitMqAddUserConsumer Get_consumer()
    {
        return _consumer;
    }

    // Implementação correta do StartAsync sem parâmetros adicionais
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Criar um escopo de serviço para o RabbitMqAddUserConsumer
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var contactRepository = scope.ServiceProvider.GetRequiredService<IContactRepository>();
                var regionRepository = scope.ServiceProvider.GetRequiredService<IRegionRepository>();

                // Criar e iniciar o consumidor
                _consumer = new RabbitMqAddUserConsumer(contactRepository, regionRepository);
                _consumer.Start(); // Inicia o consumidor

                _logger.LogInformation("RabbitMqAddUserConsumer iniciado com sucesso.");
            }
        }
        catch (Exception ex)
        {
            // Log de erro
            _logger.LogError(ex, "Erro ao iniciar o RabbitMqAddUserConsumer.");
        }
    }

    // Método para parar o serviço
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Caso precise de lógica de parada do consumer, ela deve ser implementada aqui
            _consumer?.Stop(); // Supondo que você tenha um método Stop no RabbitMqAddUserConsumer

            _logger.LogInformation("RabbitMqAddUserConsumer parado com sucesso.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar o RabbitMqAddUserConsumer.");
        }
    }
}

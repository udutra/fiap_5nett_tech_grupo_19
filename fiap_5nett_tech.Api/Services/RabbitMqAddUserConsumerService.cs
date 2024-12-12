using fiap_5nett_tech.api.Consumers;
using fiap_5nett_tech.Domain.Repositories;

namespace fiap_5nett_tech.api.Services;

public class RabbitMqAddUserConsumerService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    
    private RabbitMqAddUserConsumer _consumer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var s = serviceScopeFactory.CreateScope())
        {
            var contact = s.ServiceProvider.GetRequiredService<IContactRepository>();
            var region = s.ServiceProvider.GetRequiredService<IRegionRepository>();
            _consumer = new RabbitMqAddUserConsumer(contact, region);
            _consumer.Start();
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
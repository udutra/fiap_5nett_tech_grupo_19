using fiap_5nett_tech.Application.Interface;

namespace fiap_5nett_tech.Api.Contact.Read.Services;

/// <summary>
/// 
/// </summary>
/// <param name="scopeFactory"></param>
public class RabbitMqReadContactConsumerCs(IServiceScopeFactory scopeFactory) : IHostedService
{
    private Worker _consumer;
    private IServiceScope _scope;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = scopeFactory.CreateScope();
        var contactService = _scope.ServiceProvider.GetRequiredService<IContactInterface>();
        var serviceScopeFactory = _scope.ServiceProvider.GetService<IServiceScopeFactory>();

        _consumer = new Worker(contactService, serviceScopeFactory);
        _consumer.Start();

        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
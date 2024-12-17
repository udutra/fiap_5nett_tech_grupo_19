namespace fiap_5nett_tech.Domain.RabbitMqConfiguration;

public class RoutingKeyConfiguration
{
    public const string RoutingQueueCreate = "contact.created";
    public const string RoutingReadCreate = "contact.read";
    public const string RoutingQueueDelete = "contact.deleted";
    public const string RoutingQueueUpdate = "contact.updated";
}
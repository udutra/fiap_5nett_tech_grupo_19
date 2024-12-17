namespace fiap_5nett_tech.Domain.RabbitMqConfiguration;

public static class RoutingKeyConfiguration
{
    public const string RoutingQueueCreate = "contact.created";
    public const string RoutingReadCreateGetOneById = "contact.read.get.one.by.id";
    public const string RoutingReadCreateGetOneByDddAndPhone = "contact.read.get.one.by.DddAndPhone";
    public const string RoutingReadCreateGetAll = "contact.read.get.all";
    public const string RoutingReadCreateGetAllByDdd = "contact.read.get.all.by.Ddd";
    public const string RoutingQueueDelete = "contact.deleted";
    public const string RoutingQueueUpdate = "contact.updated";
}
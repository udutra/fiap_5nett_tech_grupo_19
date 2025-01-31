namespace fiap_5nett_tech.Api.CreateContact.Domain.RabbitMqConfiguration;

public static class RoutingKeyConfiguration
{
    public const string RoutingQueueCreate = "contact.created";
    public const string RoutingQueueReadGetOneById = "contact.read.get.one.by.id";
    public const string RoutingQueueReadGetOneByDddAndPhone = "contact.read.get.one.by.DddAndPhone";
    public const string RoutingQueueReadGetAll = "contact.read.get.all";
    public const string RoutingQueueReadGetAllByDdd = "contact.read.get.all.by.Ddd";
    public const string RoutingQueueDelete = "contact.deleted";
    public const string RoutingQueueUpdate = "contact.updated";
}
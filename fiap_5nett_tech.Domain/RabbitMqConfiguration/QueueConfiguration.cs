namespace fiap_5nett_tech.Domain.RabbitMqConfiguration;

public class QueueConfiguration
{
    public const string ContactCreatedQueue = "contact_created_queue";
    public const string ContactCreatedQueueReturn = "contact_created_queue_return";
    public const string ContactReadQueue = "contact_read_queue";
    public const string ContactReadQueueReturn = "contact_read_queue_return";
    public const string ContactUpdatedQueue = "contact_updated_queue";
    public const string ContactUpdatedReturn = "contact_updated_queue_return";
    public const string ContactDeletedQueue = "contact_deleted_queue";
    public const string ContactDeletedReturn = "contact_deleted_queue_return";
}
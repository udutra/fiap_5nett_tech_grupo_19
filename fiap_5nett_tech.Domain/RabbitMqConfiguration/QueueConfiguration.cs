namespace fiap_5nett_tech.Domain.RabbitMqConfiguration;

public static class QueueConfiguration
{
    public const string ContactCreatedQueue = "contact_created_queue";
    public const string ContactCreatedQueueReturn = "contact_created_queue_return";
    
    public const string ContactReadQueueGetOneById = "contact_read_queue_get_one_by_id";
    public const string ContactReadQueueGetOneByIdReturn = "contact_read_queue_get_one_by_id_return";
    
    public const string ContactReadQueueGetOneByDddAndPhone = "contact_read_queue_get_one_by_ddd_and_phone";
    public const string ContactReadQueueGetOneByDddAndPhoneReturn = "contact_read_queue_get_one_by_ddd_and_phone_return";
    
    public const string ContactReadQueueGetAll = "contact_read_queue_get_all";
    public const string ContactReadQueueGetAllReturn = "contact_read_queue_get_all_return";
    
    public const string ContactReadQueueGetAllByDdd = "contact_read_queue_get_all_by_ddd";
    public const string ContactReadQueueGetGetAllByDddReturn = "contact_read_queue_get_all_by_ddd_return";
    
    public const string ContactUpdatedQueue = "contact_updated_queue";
    public const string ContactUpdatedQueueReturn = "contact_updated_queue_return";
    
    public const string ContactDeletedQueue = "contact_deleted_queue";
    public const string ContactDeletedQueueReturn = "contact_deleted_queue_return";
}
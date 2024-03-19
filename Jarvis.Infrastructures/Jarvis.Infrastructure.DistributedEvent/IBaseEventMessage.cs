namespace Jarvis.Infrastructure.DistributedEvent;

public interface IBaseEventMessage
{
    /// <summary>
    /// Id of the record in the database
    /// </summary>
    /// <value></value>
    string Id { get; set; }

    string Action { get; set; }
    string EntityName { get; set; }
    object EntityData { get; set; }
    string Sender { get; set; }
}
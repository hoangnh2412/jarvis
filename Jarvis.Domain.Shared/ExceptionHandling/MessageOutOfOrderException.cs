namespace Jarvis.Domain.Shared.ExceptionHandling;

public class MessageOutOfOrderException : Exception
{
    public string MessageId { get; set; }
    public string CorrelationId { get; set; }
    public long SequenceNumber { get; set; }
    public long ExpectedSequenceNumber { get; set; }

    public MessageOutOfOrderException(string messageId, string correlationId, long sequenceNumber, long expectedSequenceNumber)
        : base($"Message [MessageId: {messageId}] - [CorrelationId: {correlationId}] is out of order. Expected sequence number is {expectedSequenceNumber}, actually recieved {sequenceNumber}")
    {
        MessageId = messageId;
        CorrelationId = correlationId;
        SequenceNumber = sequenceNumber;
        ExpectedSequenceNumber = expectedSequenceNumber;
    }
}

using Experimental.System.Messaging;

namespace Infrastructure.Message.Msmq
{
    public class MsmqClient : IMsmqClient
    {
        public void Purge(string name)
        {
            using var queue = new MessageQueue(name);
            queue.Purge();
        }

        public void Send(string name, string message)
        {
            using var queue = new MessageQueue(name);
            queue.DefaultPropertiesToSend.Recoverable = true;
            queue.Send(message);
        }
    }
}

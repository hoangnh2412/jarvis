using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitQueueOption
    {
        public string Name { get; set; }
        public string ConnectionName { get; set; }
        public int NumberOfConsumer { get; set; }
        public string QueueName { get; set; }

        public RabbitInputQueueOption Input { get; set; }
        public RabbitOutputQueueOption Output { get; set; }
    }
}

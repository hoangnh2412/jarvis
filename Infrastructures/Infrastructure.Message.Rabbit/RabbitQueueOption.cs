using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitQueueOption
    {
        public string ConnectionName { get; set; }
        public int NumberOfConsumer { get; set; }
    }
}

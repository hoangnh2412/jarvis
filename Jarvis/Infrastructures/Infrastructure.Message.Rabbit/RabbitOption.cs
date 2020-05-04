using System;
using System.Collections.Generic;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitOption
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public List<RabbitQueueOption> Queues { get; set; }
    }
}

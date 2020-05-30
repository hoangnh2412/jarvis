using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitServiceContext
    {
        public IModel Channel { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Message.Rabbit
{
    public class RabbitResponseModel<T>
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; }
        public bool Succeeded { get; set; }
        public Exception Exception { get; set; }
        public T Data { get; set; }
    }
}

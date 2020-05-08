using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Infrastructure.Message.Kafka
{
    public interface IKafkaClient
    {
        Task<DeliveryResult<string, string>> ProduceAsync(string topic, string key, string val);

        Task CleanAsync(IList<string> topics);
    }
}
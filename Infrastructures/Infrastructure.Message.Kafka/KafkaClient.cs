using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace Infrastructure.Message.Kafka
{
    public class KafkaClient : IKafkaClient
    {
        private readonly IProducer<string, string> _producer;
        private readonly IAdminClient _adminClient;

        public KafkaClient(ProducerConfig producerConfig, AdminClientConfig adminClientConfig)
        {
            _producer = new ProducerBuilder<string, string>(producerConfig).Build();
            _adminClient = new AdminClientBuilder(adminClientConfig).Build();
        }

        public async Task<DeliveryResult<string, string>> ProduceAsync(string topic, string key, string val)
        {
            var message = new Message<string, string>
            {
                Key = key,
                Value = val
            };

            var result = await _producer.ProduceAsync(topic, message);
            return result;
        }

        public async Task CleanAsync(IList<string> topics)
        {
            await _adminClient.DeleteTopicsAsync(topics, null);
        }
    }
}

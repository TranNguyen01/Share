using Confluent.Kafka;

namespace Moto.Services
{
    public class MotoProducer
    {
        private readonly ProducerConfig _config;

        public MotoProducer()
        {
            _config = new ProducerConfig()
            {
                BootstrapServers = "pkc-lzvrd.us-west4.gcp.confluent.cloud:9092",
                SecurityProtocol = SecurityProtocol.SaslSsl,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = "37TGLFSWA3CNWHX3",
                SaslPassword = "wZwiHlWhdC8+cHoKNw2U/qM1tznx0NpdfH0XLcQo+Ss2pOpnBaBfA3UH6sx0ZD1/"
            };
        }

        public async Task SendMessage(string topic, string value)
        {
            using var producer = new ProducerBuilder<Null, string>(_config).Build();
            var response = await producer.ProduceAsync(topic,
                new Message<Null, string>
                {
                    Value = value
                }
            );
        }
    }
}

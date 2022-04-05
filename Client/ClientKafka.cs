using Confluent.Kafka;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class ClientKafka : IDisposable
    {
        private const string _kafkaHost = "127.0.0.1";
        private readonly IProducer<string, string> producer;

        public ClientKafka()
        {
            var producer_config = new ProducerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                ClientId = Dns.GetHostName(),
            };

            producer = new ProducerBuilder<string, string>(producer_config.AsEnumerable()).Build();
        }

        public async Task Raise(string topic, string value)
        {
            var task = producer.ProduceAsync(topic, new Message<string, string> { Value = value })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"Failed to deliver message (CreateTicket): value = {value}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced event (CreateTicket) to topic {topic}: value = {value}");
                    }
                });

            await task;
            producer.Flush(TimeSpan.FromSeconds(10));
        }

        public void Dispose()
        {
            producer.Dispose();
        }
    }
}

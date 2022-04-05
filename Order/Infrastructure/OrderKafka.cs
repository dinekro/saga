using Confluent.Kafka;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class OrderKafka
    {
        private const string _kafkaHost = "127.0.0.1";
        private readonly IProducer<string, string> _producer;

        public OrderKafka()
        {
            var producer_config = new ProducerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                ClientId = Dns.GetHostName(),
            };

            _producer = new ProducerBuilder<string, string>(producer_config.AsEnumerable()).Build();
        }

        public async Task Raise(string topic, string value)
        {
            var task = _producer.ProduceAsync(topic, new Message<string, string> { Value = value })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine($"kafka {topic} failed");
                    }
                    else
                    {
                        Console.WriteLine($"kafka {topic} produced");
                    }
                });
            await task;

            _producer.Flush(TimeSpan.FromSeconds(10));
        }

        public async Task Listen(string topic, CancellationToken ct, Func<string, Task> func)
        {
            var counsumer_config = new ConsumerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            
            using var consumer = new ConsumerBuilder<string, string>(counsumer_config.AsEnumerable()).Build();

            consumer.Subscribe(topic);
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(ct);
                    Console.WriteLine($"kafka {topic} consumed");
                    await func(cr.Message.Value);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Ctrl-C was pressed");
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}

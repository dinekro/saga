using Confluent.Kafka;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen
{
    internal class Kafka
    {
        //private const string _kafkaHost = "172.17.0.1";
        private const string _kafkaHost = "127.0.0.1";
        private readonly IConsumer<string, string> consumer;
        private readonly KitchenRepository _kitchenRepository;
        const string topic = "new-topic1";

        public Kafka(KitchenRepository kitchenRepository)
        {
            _kitchenRepository = kitchenRepository;

            var counsumer_config = new ConsumerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            consumer = new ConsumerBuilder<string, string>(counsumer_config.AsEnumerable()).Build();
        }

        public async Task CheckTicketCreated()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            consumer.Subscribe(topic);
            try
            {
                while (true)
                {
                    var cr = consumer.Consume(cts.Token);

                    Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                    
                    if(int.TryParse(cr.Message.Value, out int orderId))
                        await _kitchenRepository.CreateTicket(orderId);
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
            Console.ReadLine();
        }

        public static async Task Start()
        {
            var producer_config = new ProducerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                ClientId = Dns.GetHostName(),
            };

            const string topic = "new-topic";

            string[] users = { "eabara", "jsmith", "sgarcia", "jbernard", "htanaka", "awalther" };
            string[] items = { "book", "alarm clock", "t-shirts", "gift card", "batteries" };

            using (var producer = new ProducerBuilder<string, string>(producer_config.AsEnumerable()).Build())
            {
                var numProduced = 0;
                const int numMessages = 10;
                for (int i = 0; i < numMessages; ++i)
                {
                    Random rnd = new Random();
                    var user = users[rnd.Next(users.Length)];
                    var item = items[rnd.Next(items.Length)];


                    var task = producer.ProduceAsync("new-topic", new Message<string, string> { Key = user, Value = item })
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted)
                            {
                                Console.WriteLine($"Failed to deliver message: ");
                            }
                            else
                            {
                                Console.WriteLine($"Produced event to topic {topic}: key = {user,-10} value = {item}");
                                numProduced += 1;
                            }
                        });

                    await task;
                }

                producer.Flush(TimeSpan.FromSeconds(10));
                Console.WriteLine($"{numProduced} messages were produced to topic {topic}");
            }





            var counsumer_config = new ConsumerConfig
            {
                BootstrapServers = $"{_kafkaHost}:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };


            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            using (var consumer = new ConsumerBuilder<string, string>(counsumer_config.AsEnumerable()).Build())
            {
                consumer.Subscribe(topic);
                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed event from topic {topic} with key {cr.Message.Key,-10} and value {cr.Message.Value}");
                        await Task.Delay(500);
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
                Console.ReadLine();
            }
        }
    }
}

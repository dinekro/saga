using Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Consumer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var kafka = new ConsumerKafka();

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            await Task.Run(async () =>
            {
                await kafka.Listen("verify-consumer", cts.Token, async orderId =>
                {
                    await kafka.Raise("consumer-verified", "true");
                });
            });

            Console.ReadLine();
        }
    }
}

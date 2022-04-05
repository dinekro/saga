using Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Accounting
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var kafka = new AccountingKafka();

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            await Task.Run(async () =>
            {
                await kafka.Listen("authorize-card", cts.Token, async orderId =>
                {
                    await kafka.Raise("card-authorized", "true");
                });
            });
        }
    }
}

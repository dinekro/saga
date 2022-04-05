using Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var repository = new KitchenRepository();
            var kafka = new KitchenKafka();

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            new Task(async () =>
            {
                await kafka.Listen("create-ticket", cts.Token, async result =>
                {
                    if (!int.TryParse(result, out int orderId))
                        return;

                    var ticketId = await repository.CreateTicket(orderId);
                    await kafka.Raise("ticket-created", ticketId.ToString());
                });
            }).Start();

            new Task(async () =>
            {
                await kafka.Listen("approve-ticket", cts.Token, async result =>
                {
                    Console.WriteLine("ticket approved");

                    await Task.Delay(1);
                });
            }).Start();

            Console.ReadLine();
        }
    }
}

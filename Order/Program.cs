using Confluent.Kafka;
using Domain;
using Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Order
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var kafka = new OrderKafka();
            var orderRepository = new OrderRepository();
            CreateOrderSaga createOrderSaga = null;


            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                cts.Cancel();
            };

            new Task(async () =>
             {
                 await kafka.Listen("customer-sent-new-order", cts.Token, async result =>
                 {
                     if (!int.TryParse(result, out int consumerId))
                         return;

                     var orderId = await orderRepository.CreateOrderAsync(consumerId);
                     createOrderSaga = new CreateOrderSaga(kafka) { ConsumerId = consumerId, OrderId = orderId };

                     // 1
                     await createOrderSaga.VerifyConsumerAsync(orderId);
                 });
             }).Start();

            new Task(async () =>
            {
                // 2
                await kafka.Listen("consumer-verified", cts.Token, async result =>
                {
                    if (!bool.TryParse(result, out bool isVerified))
                        return;

                    if (!isVerified)
                        return;

                    // 3
                    await createOrderSaga.CreateTicketAsync(createOrderSaga.OrderId);
                });
            }).Start();


            new Task(async () =>
            {
                // 4
                await kafka.Listen("ticket-created", cts.Token, async result =>
                {
                    if (!int.TryParse(result, out int ticketId))
                        return;

                    // 5
                    await createOrderSaga.AuthorizeCardAsync(createOrderSaga.ConsumerId);
                });
            }).Start();

            new Task(async () =>
            {
                // 6
                await kafka.Listen("card-authorized", cts.Token, async result =>
                {
                    if (!bool.TryParse(result, out bool isAuthorizes))
                        return;

                    // 7
                    await createOrderSaga.ApproveTicketAsync(createOrderSaga.ConsumerId);

                    // 8
                    await createOrderSaga.ApproveOrderAsync(createOrderSaga.ConsumerId);
                });
            }).Start();

            new Task(async () =>
            {
                await kafka.Listen("approve-order", cts.Token, async result =>
                {
                    Console.WriteLine("order approved");

                    await Task.Delay(1);
                });
            }).Start();

            Console.ReadLine();
        }
    }
}

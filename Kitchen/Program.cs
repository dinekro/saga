using System;
using System.Threading.Tasks;

namespace Kitchen
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var repository = new KitchenRepository();
            var ticket = new Kafka(repository);
            await ticket.CheckTicketCreated();
            //await repository.CreateTicket(1);
        }
    }
}

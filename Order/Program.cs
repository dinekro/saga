using Microsoft.EntityFrameworkCore;
using Ticket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Kitchen;

namespace Ticket
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var saga = new CreateOrderSaga();
            await saga.Run();


            //await Kafka.Start();
            Console.ReadLine();
        }
    }

    class CreateOrderSaga
    {
        //private const string _host = "172.17.0.1";
        private const string _host = "127.0.0.1";
        public static MainContext GetContext()
        {
            string CatalogPostgresConnectionString = $"Host={_host};Port=5432;Database=saga_order;Username=postgres;Password=qwerty123";

            var optionsPg = new DbContextOptionsBuilder<MainContext>()
                .UseNpgsql(CatalogPostgresConnectionString)
                .Options;
            return new MainContext(optionsPg);
        }

        public async Task Run()
        {
            // createOrder - state:panding (create in bd)
            // verifyConsumer - return true
            // createTicket - state:panding (create in bd)
            // authorizeCard - return true
            // approveTichet - state:approve (update bd)
            // approveOrder - state:approve (update bd)


            var consumerId = 223;
            var orderId = await CreateOrderAsync(consumerId);
            var isConsumerVerified = VerifyConsumer(consumerId);

            if (!isConsumerVerified)
            {
                RevertOrderCreation(orderId);
            }

            var ticketId = CreateTicket(orderId);
            var isCardAuthorized = AuthorizeCard(consumerId);

            if (!isCardAuthorized)
            {
                RevertOrderCreation(orderId);
                RevertTicketCreation(ticketId);
            }

            ApproveTicket(ticketId);
            await ApproveOrder(orderId);
        }

        public bool RevertOrderCreation(int orderId)
        {
            var result = true;
            Console.WriteLine($"RevertOrderCreation - orderId: {orderId}, result: {result}");
            return result;
        }

        public bool RevertTicketCreation(int ticketId)
        {
            var result = true;
            Console.WriteLine($"RevertTicketCreation - ticketId: {ticketId}, result: {result}");
            return result;
        }

        public async Task<int> CreateOrderAsync(int consumerId)
        {
            using var context = GetContext();
            var sql = $"INSERT INTO orders (status,consumer_id) VALUES({(int)OrderStatus.Pending}, {consumerId}) returning id,status,consumer_id";
            var order = (await context.Orders.FromSqlRaw(sql)
                .ToListAsync()).First();
            Console.WriteLine($"CreateOrder - consumerId: {consumerId}, result: {order.Id}");
            return order.Id;
        }

        public bool VerifyConsumer(int consumerId)
        {
            var result = true;
            Console.WriteLine($"VerifyConsumer - consumerId: {consumerId}, result: {result}");
            return true;
        }

        public int CreateTicket(int orderId)
        {
            var k = new Kafka();
            k.CreateTicket(orderId).Wait();

            var ticketId = 1;
            Console.WriteLine($"CreateTicket - orderId: {orderId}, result: {ticketId}");
            return ticketId;
        }

        public bool AuthorizeCard(int consumerId)
        {
            var result = true;
            Console.WriteLine($"AuthorizeCard - consumerId: {consumerId}, result: {result}");
            return true;
        }

        public void ApproveTicket(int ticketId)
        {
            Console.WriteLine($"ApproveTicket - ticketId: {ticketId}");
        }

        public async Task ApproveOrder(int orderId)
        {
            using var context = GetContext();
            var sql = $"UPDATE public.orders SET status={(int)OrderStatus.Created} WHERE id={orderId} returning id,status,consumer_id";
            await context.Orders.FromSqlRaw(sql).ToListAsync();
            Console.WriteLine($"ApproveOrder - orderId: {orderId}");
        }
    }
}

public class MainContext : DbContext
{
    public MainContext()
    {
    }

    public MainContext(DbContextOptions<MainContext> options)
        : base(options)
    {
    }

    public DbSet<OrderDb> Orders { get; set; }
}

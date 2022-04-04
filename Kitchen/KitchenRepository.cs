using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kitchen
{
    class KitchenRepository
    {
        private const string _host = "127.0.0.1";
        public static MainContext GetContext()
        {
            string CatalogPostgresConnectionString = $"Host={_host};Port=5432;Database=Saga_Kitchen;Username=postgres;Password=qwerty123";


            var optionsPg = (new DbContextOptionsBuilder<MainContext>())
                .UseNpgsql(CatalogPostgresConnectionString)
                .Options;
            return new MainContext(optionsPg);
        }

        public bool RevertTicketCreation(int ticketId)
        {
            var result = true;
            Console.WriteLine($"RevertTicketCreation - ticketId: {ticketId}, result: {result}");
            return result;
        }

        public async Task<int> CreateTicket(int orderId)
        {
            using var context = GetContext();
            var sql = $"INSERT INTO tickets (status,order_id) VALUES({(int)TicketStatus.Pending}, {orderId}) returning id,status,order_id";
            var ticket = (await context.Tickets.FromSqlRaw(sql)
                .ToListAsync()).First();
            Console.WriteLine($"CreateTicket - orderId: {orderId}, result: {ticket.Id}");
            return ticket.Id;
        }

        public async Task ApproveTicket(int ticketId)
        {
            using var context = GetContext();
            var sql = $"UPDATE public.tickets SET status={(int)TicketStatus.Created} WHERE id={ticketId} returning id,status,order_id";
            await context.Tickets.FromSqlRaw(sql).ToListAsync();
            Console.WriteLine($"ApproveTicket - ticketId: {ticketId}");
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

        public DbSet<TicketDb> Tickets { get; set; }
    }
}

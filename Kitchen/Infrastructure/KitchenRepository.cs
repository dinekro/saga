using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    class KitchenRepository
    {
        private const string _host = "127.0.0.1";
        public static MainContext GetContext()
        {
            string CatalogPostgresConnectionString = $"Host={_host};Port=5432;Database=template1;Username=admin;Password=example";


            var optionsPg = (new DbContextOptionsBuilder<MainContext>())
                .UseNpgsql(CatalogPostgresConnectionString)
                .Options;
            return new MainContext(optionsPg);
        }

        public async Task<int> CreateTicket(int orderId)
        {
            using var context = GetContext();
            var sql = $"INSERT INTO tickets (status,order_id) VALUES({(int)TicketStatus.Pending}, {orderId}) returning id,status,order_id";
            var ticket = (await context.Tickets.FromSqlRaw(sql)
                .ToListAsync()).First();
            Console.WriteLine($"postgre create ticket - orderId: {orderId}, result: {ticket.Id}");
            return ticket.Id;
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

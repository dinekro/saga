using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure
{
    class OrderRepository
    {
        private const string _host = "127.0.0.1";
        public static MainContext GetContext()
        {
            string CatalogPostgresConnectionString = $"Host={_host};Port=5432;Database=template1;Username=admin;Password=example";

            var optionsPg = new DbContextOptionsBuilder<MainContext>()
                .UseNpgsql(CatalogPostgresConnectionString)
                .Options;
            return new MainContext(optionsPg);
        }

        public async Task<int> CreateOrderAsync(int consumerId)
        {
            using var context = GetContext();
            var sql = $"INSERT INTO orders (status,consumer_id) VALUES({(int)OrderStatus.Pending}, {consumerId}) returning id,status,consumer_id";
            var order = (await context.Orders.FromSqlRaw(sql)
                .ToListAsync()).First();
            Console.WriteLine($"postgre create order - consumerId: {consumerId}, result: {order.Id}");
            return order.Id;
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
}

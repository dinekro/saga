using Infrastructure;
using System.Threading.Tasks;

namespace Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var kafka = new ClientKafka();

            var clientId = 1;
            await kafka.Raise("customer-sent-new-order", clientId.ToString());
        }
    }
}

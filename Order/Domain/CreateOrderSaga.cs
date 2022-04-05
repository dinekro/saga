using Infrastructure;
using System.Threading.Tasks;

namespace Domain
{
    class CreateOrderSaga
    {
        private OrderKafka _kafka;

        public CreateOrderSaga(OrderKafka kafka)
        {
            _kafka = kafka;
        }

        public int ConsumerId { get; init; }
        public int OrderId { get; init; }

        public async Task VerifyConsumerAsync(int orderId)
        {
            await _kafka.Raise("verify-consumer", orderId.ToString());
        }

        public async Task CreateTicketAsync(int orderId)
        {
            await _kafka.Raise("create-ticket", orderId.ToString());
        }

        public async Task AuthorizeCardAsync(int orderId) {
            await _kafka.Raise("authorize-card", orderId.ToString());
        }

        public async Task ApproveTicketAsync(int ticketId)
        {
            await _kafka.Raise("approve-ticket", ticketId.ToString());
        }

        public async Task ApproveOrderAsync(int orderId)
        {
            await _kafka.Raise("approve-order", orderId.ToString());
        }
    }
}

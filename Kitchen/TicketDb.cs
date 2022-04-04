using System.ComponentModel.DataAnnotations.Schema;

namespace Kitchen
{
    public class TicketDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("status")]
        public TicketStatus Status { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }
    }

    public enum TicketStatus
    {
        Pending = 0,
        Created = 1,
    }
}

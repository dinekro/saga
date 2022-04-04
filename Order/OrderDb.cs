using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ticket
{
    public class OrderDb
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; }

        [Column("consumer_id")]
        public int ConsumerId { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Created = 1,
    }
}

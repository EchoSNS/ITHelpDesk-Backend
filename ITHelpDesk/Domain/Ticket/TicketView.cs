using System.ComponentModel.DataAnnotations.Schema;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace ITHelpDesk.Domain.Ticket
{
    public class TicketView
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }
        public string UserId { get; set; }
        public DateTime ViewedAt { get; set; }


        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }

        public virtual ApplicationUser User { get; set; }
    }
}

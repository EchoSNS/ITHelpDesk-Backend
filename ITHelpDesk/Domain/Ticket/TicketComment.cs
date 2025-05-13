using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ITHelpDesk.Domain.Ticket
{
    public class TicketComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]  // Limit the size of content, adjust length as necessary
        public string Content { get; set; }

        [JsonIgnore]
        [ForeignKey("Ticket")]
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Ensure CreatedAt is always set
    }
}

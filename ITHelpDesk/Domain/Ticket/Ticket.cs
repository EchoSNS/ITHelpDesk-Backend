using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ITHelpDesk.Domain.Ticket
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [Required]
        public required string Description { get; set; }

        public TicketPriority Priority { get; set; }

        public TicketStatus Status { get; set; }

        public string Category { get; set; }

        [ForeignKey("Submitter")]
        public string? SubmitterId { get; set; }
        public ApplicationUser? Submitter { get; set; }

        [ForeignKey("AssignedTo")]
        public string? AssignedToId { get; set; }
        public ApplicationUser? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string? ResolutionNotes { get; set; }


        public bool IsViewed { get; set; } = false;


        public ICollection<TicketComment>? Comments { get; set; }
        public virtual ICollection<TicketView> TicketViews { get; set; } = new List<TicketView>();
    }
}

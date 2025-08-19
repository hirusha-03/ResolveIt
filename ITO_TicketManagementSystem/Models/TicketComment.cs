namespace ITO_TicketManagementSystem.Models
{
    public class TicketComment
    {
        public int TicketCommentId { get; set; }

        // Foreign key to Ticket
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to ApplicationUser
        public string CreatedByUserId { get; set; } = string.Empty;
        public ApplicationUser CreatedByUser { get; set; } = null!;
    }
}

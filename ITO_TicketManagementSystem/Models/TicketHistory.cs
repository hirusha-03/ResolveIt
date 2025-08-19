using System;

namespace ITO_TicketManagementSystem.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        //  Foreign key to Ticket
        public int? TicketId { get; set; }
        public Ticket Ticket { get; set; }

        // Action details
        public string Action { get; set; }   // e.g., "Assigned", "Status Changed", "Created"
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        // Who made the change
        public string ChangedByUserId { get; set; }
        public ApplicationUser ChangedByUser { get; set; }

        // When change was made
        public DateTime ChangedAt { get; set; }
    }
}

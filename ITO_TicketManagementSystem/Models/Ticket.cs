using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ITO_TicketManagementSystem.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "New";

        [Required]
        public string AssignType { get; set; } = "Other";

        public string? Attachment { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [BindNever, ValidateNever]
        public ApplicationUser? CreatedByUser { get; set; }

        // ✅ Necessary foreign key for assignment
        public string? AssignedToUserId { get; set; }

        // ✅ Navigation property to ApplicationUser
        [BindNever, ValidateNever]
        public ApplicationUser? AssignedToUser { get; set; }

        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
        public ICollection<TicketHistory> Histories { get; set; } = new List<TicketHistory>();
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // ✅ UI-only, not stored in DB
        [NotMapped]
        public string? AssignedToUserRole { get; internal set; }

        public Ticket() { }

        public Ticket(string title, string description, string assignType = "Other", string status = "New", string? attachment = null)
        {
            Title = title;
            Description = description;
            AssignType = assignType;
            Status = status;
            Attachment = attachment;
        }
    }
}

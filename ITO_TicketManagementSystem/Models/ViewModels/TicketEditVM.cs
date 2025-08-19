using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class TicketEditVM
    {
        [Required]
        public int TicketId { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string AssignType { get; set; } = "Other";

        [Required]
        public string Status { get; set; } = "New";

        // read-only display of the current attachment (if any)
        public string? ExistingAttachment { get; set; }

        // OPTION A: replace with a new upload
        public IFormFile? AttachmentFile { get; set; }

        // OPTION B: replace with a URL/path string
        public string? Attachment { get; set; }

        // remove the existing attachment entirely
        public bool RemoveAttachment { get; set; }
    }
}

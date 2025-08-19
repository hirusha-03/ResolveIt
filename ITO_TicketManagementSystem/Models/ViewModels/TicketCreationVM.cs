using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class TicketCreationVM
    {
        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        // You’ll default this to "New" in the form or controller too
        public string Status { get; set; } = "New";

        [Required]
        public string AssignType { get; set; } = "Other";

        // OPTION 1 (optional): user pastes a URL/path (kept as-is)
        public string? Attachment { get; set; }

        // OPTION 2 (optional): user uploads a file; we’ll save to wwwroot/uploads and
        // store the saved relative path in the Ticket entity’s Attachment field.
        public IFormFile? AttachmentFile { get; set; }
    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class AssignTicketVM
    {
        public int TicketId { get; set; }
        public string TicketTitle { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
        public List<SelectListItem> Engineers { get; set; } = new();
    }

}

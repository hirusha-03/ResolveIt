using System.Collections.Generic;

namespace ITO_TicketManagementSystem.Models.ViewModels
{
    public class TicketDetailsVM
    {
        public Ticket Ticket { get; set; }
        public IEnumerable<TicketHistory> History { get; set; }
        public IEnumerable<TicketComment> Comments { get; set; }
    }
}

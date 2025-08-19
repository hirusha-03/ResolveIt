using Microsoft.AspNetCore.Identity;

namespace ITO_TicketManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string? FullName { get; set; }
        public string? Department { get; set; }
    }
}

using ITO_TicketManagementSystem.Data;
using ITO_TicketManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ITO_TicketManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            MyAppContext context,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Dashboard(CancellationToken ct)
        {
            var currentUserId = _userManager.GetUserId(User);
            var today = DateTime.Now.Date;   // ✅ declare once here

            if (User.IsInRole("Engineering Team"))
            {
                var myTickets = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.AssignedToUserId == currentUserId)
                    .OrderByDescending(t => t.TicketId)
                    .ToListAsync(ct);

                // card counts for engineers
                ViewBag.AssignedToMe = myTickets.Count;
                ViewBag.Pending = myTickets.Count(t => t.Status == "Pending");
                ViewBag.ResolvedToday = myTickets.Count(
                    t => t.Status == "Resolved" &&
                         t.UpdatedAt.HasValue &&
                         t.UpdatedAt.Value.Date == today
                );

                return View(myTickets);
            }
            else if (User.IsInRole("Employees"))
            {
                var myTickets = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Where(t => t.CreatedByUserId == currentUserId || t.AssignedToUserId == currentUserId)
                    .OrderByDescending(t => t.TicketId)
                    .ToListAsync(ct);

                // card counts for employees
                ViewBag.MyTickets = myTickets.Count;
                ViewBag.Pending = myTickets.Count(t => t.Status == "Pending");
                ViewBag.ResolvedToday = myTickets.Count(
                    t => t.Status == "Resolved" &&
                         t.UpdatedAt.HasValue &&
                         t.UpdatedAt.Value.Date == today
                );

                return View(myTickets);
            }
            else if (User.IsInRole("Help Desk Team") || User.IsInRole("Admin"))
            {
                var allTickets = await _context.Tickets
                    .AsNoTracking()
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.AssignedToUser)
                    .Include(t => t.Comments).ThenInclude(c => c.CreatedByUser)
                    .OrderByDescending(t => t.TicketId)
                    .ToListAsync(ct);

                // 🔹 card counts for help desk/admin
                ViewBag.Unassigned = allTickets.Count(t => string.IsNullOrEmpty(t.AssignedToUserId));
                ViewBag.AssignedToMe = allTickets.Count(t => t.AssignedToUserId == currentUserId);
                ViewBag.Pending = allTickets.Count(t => t.Status == "Pending");
                ViewBag.ResolvedToday = allTickets.Count(
                    t => t.Status == "Resolved" &&
                         t.UpdatedAt.HasValue &&
                         t.UpdatedAt.Value.Date == today
                );

                // 🔹 Additional admin handling
                if (User.IsInRole("Admin"))
                {
                    var allUsers = _userManager.Users.ToList();
                    var unassigned = new List<ApplicationUser>();
                    var withRoles = new List<ApplicationUser>();

                    foreach (var u in allUsers)
                    {
                        var roles = await _userManager.GetRolesAsync(u);
                        if (roles == null || roles.Count == 0)
                            unassigned.Add(u);
                        else
                            withRoles.Add(u);
                    }

                    ViewBag.UnassignedUsers = unassigned;
                    ViewBag.UsersWithRoles = withRoles;
                }

                return View(allTickets);
            }

            // fallback if no role matched
            return View(new List<Ticket>());
        }
    }
}

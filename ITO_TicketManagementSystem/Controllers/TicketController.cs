using AspNetCoreGeneratedDocument;
using ITO_TicketManagementSystem.Data;
using ITO_TicketManagementSystem.Models;
using ITO_TicketManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace ITO_TicketManagementSystem.Controllers
{
    public class TicketController : Controller
    {
        private readonly MyAppContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _allowedExts = new[] { ".png", ".jpg", ".jpeg", ".pdf", ".txt", ".doc", ".docx" };
        private const long _maxBytes = 5_000_000; // 5 MB

        public TicketController(MyAppContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        // ---------- CREATE ----------
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new TicketCreationVM
            {
                Status = "New",
                AssignType = "Other"
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        public async Task<IActionResult> Create(TicketCreationVM vm, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(string.Empty, "You must be signed in to create a ticket.");
            }

            if (!ModelState.IsValid) return View(vm);

            string? attachmentPath = null;

            if (vm.AttachmentFile is { Length: > 0 })
            {
                try
                {
                    attachmentPath = await SaveFileToUploadsAsync(vm.AttachmentFile, ct);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(vm.AttachmentFile), ex.Message);
                    TempData["Error"] = "File upload failed: " + ex.Message; //  error toast
                    return View(vm);
                }
            }
            else if (!string.IsNullOrWhiteSpace(vm.Attachment))
            {
                attachmentPath = vm.Attachment.Trim();
            }

            // Auto-Assign to Help Desk
            string? assignedTo = null;
            var helpDeskUsers = await _userManager.GetUsersInRoleAsync("Help Desk Team");
            if (helpDeskUsers.Any())
            {
                // Option: pick randomly
                var random = new Random();
                assignedTo = helpDeskUsers[random.Next(helpDeskUsers.Count)].Id;
            }

            var ticket = new Ticket
            {
                Title = vm.Title,
                Description = vm.Description,
                AssignType = string.IsNullOrWhiteSpace(vm.AssignType) ? "Other" : vm.AssignType,
                Status = string.IsNullOrWhiteSpace(vm.Status) ? "New" : vm.Status,
                Attachment = attachmentPath,
                CreatedByUserId = userId!,
                AssignedToUserId = assignedTo,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync(ct);

                await AddHistory(ticket.TicketId, userId!,
                    $"Ticket created with status {ticket.Status}, auto-assigned to Help Desk", ct);

                // success toast
                TempData["Success"] = $"Ticket \"{ticket.Title}\" created successfully.";

                return RedirectToAction(nameof(Display));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // error toast
                TempData["Error"] = "Error creating ticket: " + ex.Message;
                return View(vm);
            }
        }



        // ---------- READ ----------
        [HttpGet]
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        public async Task<IActionResult> Display(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            IQueryable<Ticket> query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedByUser);

            if (User.IsInRole("Employees"))
            {
                query = query.Where(t => t.CreatedByUserId == userId || t.AssignedToUserId == userId);
            }

            var tickets = await query
                .OrderByDescending(t => t.TicketId)
                .ToListAsync(ct);

            return View(tickets);
        }

        [HttpGet]
        [Authorize(Roles = "Help Desk Team, Engineering Team, Admin")]
        public async Task<IActionResult> DisplayAll(
            string? search,
            string? status,
            string? type,
            string? assignment,
            string? sort,
            bool preview = false)
        {
            var meId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedByUser)
                .AsQueryable();

            // 🔎 Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    t.Description.Contains(search));
            }

            // 📌 Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            // 📌 Type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(t => t.AssignType == type);
            }

            // 👤 Assignment filter
            if (!string.IsNullOrWhiteSpace(assignment))
            {
                if (assignment == "Assigned to me")
                {
                    query = query.Where(t => t.AssignedToUserId == meId);
                }
                else if (assignment == "Unassigned")
                {
                    query = query.Where(t => t.AssignedToUserId == null);
                }
                else if (assignment == "Assigned to others")
                {
                    query = query.Where(t => t.AssignedToUserId != null && t.AssignedToUserId != meId);
                }
            }

            // ↕ Sorting
            if (sort == "Oldest")
            {
                query = query.OrderBy(t => t.TicketId);
            }
            else if (sort == "Priority")
            {
               // query = query.OrderByDescending(t => t.Priority);
            }
            else
            {
                query = query.OrderByDescending(t => t.TicketId); // default: latest
            }

            // limit for preview
            if (preview)
            {
                query = query.Take(3);
            }

            var tickets = await query.ToListAsync();

            // preload role names for assigned users
            var userRoles = await (from ur in _context.UserRoles
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   select new { ur.UserId, RoleName = r.Name })
                                   .ToListAsync();

            foreach (var t in tickets)
            {
                if (!string.IsNullOrEmpty(t.AssignedToUserId))
                {
                    t.AssignedToUserRole = userRoles
                        .FirstOrDefault(ur => ur.UserId == t.AssignedToUserId)?.RoleName;
                }
            }

            // return partial view for AJAX
            return PartialView("_TicketListPartial", tickets);
        }




        [HttpGet]
        [Authorize(Roles = "Employees")]
        public async Task<IActionResult> DisplayCreated(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments).ThenInclude(c => c.CreatedByUser)
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.TicketId)
                .ToListAsync(ct);

            ViewBag.FilterType = "Created by Me";
            return View("Display", tickets);
        }

        [HttpGet]
        [Authorize(Roles = "Employees,Engineering Team")]
        public async Task<IActionResult> DisplayAssigned(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tickets = await _context.Tickets
                .AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Comments).ThenInclude(c => c.CreatedByUser)
                .Where(t => t.AssignedToUserId == userId)
                .OrderByDescending(t => t.TicketId)
                .ToListAsync(ct);

            ViewBag.FilterType = "Assigned to Me";
            return View("MyAssignedTickets", tickets);
        }



        // ---------- DETAILS ----------
        [HttpGet]
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.TicketId == id, ct);

            if (ticket == null) return NotFound();

            var history = await _context.TicketHistories
                .AsNoTracking()
                .Where(h => h.TicketId == id)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync(ct);

            var comments = await _context.TicketComments
                .AsNoTracking()
                .Include(c => c.CreatedByUser)
                .Where(c => c.TicketId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            var vm = new TicketDetailsVM
            {
                Ticket = ticket,
                History = history,
                Comments = comments
            };

            return View(vm); //  Now passes the right type
        }




        // ---------- COMMENTS ----------
        [HttpGet]
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        public async Task<IActionResult> GetComments(int ticketId, CancellationToken ct)
        {
            var comments = await _context.TicketComments
                .AsNoTracking()
                .Include(c => c.Ticket)            // <-- include Ticket
                .Include(c => c.CreatedByUser)     // <-- include User
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            return View("TicketComments", comments);
        }


        [HttpGet]
        [Authorize(Roles = "Help Desk Team, Engineering Team")]
        public async Task<IActionResult> AddComment(int ticketId, CancellationToken ct)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == ticketId, ct);

            if (ticket == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var currentUserName = User.Identity?.Name ?? "Unknown";

            var vm = new TicketCommentsVM
            {
                TicketId = ticket.TicketId,
                TicketTitle = ticket.Title,
                CreatedByUserId = currentUserId,
                CreatedByUserName = currentUserName
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Help Desk Team, Engineering Team")]
        public async Task<IActionResult> AddComment(TicketCommentsVM vm, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.NewComment))
            {
                ModelState.AddModelError("NewComment", "Comment cannot be empty.");
                return View(vm);
            }

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == vm.TicketId, ct);
            if (ticket == null)
            {
                TempData["ToastType"] = "danger";
                TempData["ToastMessage"] = " Ticket not found.";
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var comment = new TicketComment
            {
                TicketId = vm.TicketId,
                Comment = vm.NewComment,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync(ct);

            await AddHistory(ticket.TicketId, userId!, "Comment added", ct);

            //  Toast feedback
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = " Comment added successfully.";

            return RedirectToAction("GetComments", new { ticketId = vm.TicketId });
        }



        // ---------- UPDATE ----------
        [HttpPost]
        [Authorize(Roles = "Engineering Team,Employees,Help Desk Team")]
        public async Task<IActionResult> EditStatus(int id, string status)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            if (User.IsInRole("Employees") && ticket.AssignedToUserId != currentUserId)
            {
                return Forbid();
            }

            var oldStatus = ticket.Status;
            ticket.Status = status;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Update(ticket);
            await _context.SaveChangesAsync();

            await AddHistory(ticket.TicketId, currentUserId!, $"Status changed from {oldStatus} → {status}", CancellationToken.None);

            return RedirectToAction("Dashboard", "Home");
        }

        [HttpGet]
        [Authorize(Roles = "Help Desk Team")]
        public async Task<IActionResult> AssignTicket(int ticketId, CancellationToken ct)
        {
            var ticket = await _context.Tickets
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TicketId == ticketId, ct);

            if (ticket == null) return NotFound();

            // Load possible assignees (Engineers + Employees)
            var engineers = await _userManager.GetUsersInRoleAsync("Engineering Team");
            var employees = await _userManager.GetUsersInRoleAsync("Employees");

            var allAssignees = engineers.Concat(employees).Distinct().ToList();

            var vm = new AssignTicketVM
            {
                TicketId = ticket.TicketId,
                TicketTitle = ticket.Title,
                Engineers = allAssignees.Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = u.UserName
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Help Desk Team")]
        public async Task<IActionResult> AssignTicket(AssignTicketVM vm, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(vm.AssignedToUserId))
            {
                ModelState.AddModelError("AssignedToUserId", "Please select an engineer/employee.");
                return View(vm);
            }

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketId == vm.TicketId, ct);
            if (ticket == null)
            {
                TempData["ToastType"] = "danger";
                TempData["ToastMessage"] = "❌ Ticket not found.";
                return NotFound();
            }

            var oldAssignee = ticket.AssignedToUserId;

            ticket.AssignedToUserId = vm.AssignedToUserId;
            ticket.Status = "In Progress";
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Update(ticket);
            await _context.SaveChangesAsync(ct);

            await AddHistory(ticket.TicketId, _userManager.GetUserId(User)!,
                $"Ticket reassigned from {(oldAssignee ?? "None")} to {vm.AssignedToUserId}", ct);

            // ✅ Toast feedback
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = $"📌 Ticket #{ticket.TicketId} assigned successfully.";

            return RedirectToAction("Dashboard", "Home");
        }




        // ---------- DELETE ----------
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var ticket = await _context.Tickets.AsNoTracking().FirstOrDefaultAsync(x => x.TicketId == id, ct);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(x => x.TicketId == id, ct);
            if (ticket == null)
            {
                TempData["ToastType"] = "danger";
                TempData["ToastMessage"] = "❌ Ticket not found.";
                return NotFound();
            }

            //  Log history before deletion
            await AddHistory(ticket.TicketId, _userManager.GetUserId(User)!, "Ticket deleted", ct);

            //  Delete attachment if any
            await DeleteFromUploadsAsync(ticket.Attachment);

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);

            // 🔔 Success toast
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = $"🗑️ Ticket #{ticket.TicketId} deleted successfully.";

            return RedirectToAction(nameof(Display));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employees,Help Desk Team, Engineering Team")]
        public async Task<IActionResult> ExportPdf(int id, CancellationToken ct)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.TicketId == id, ct);

            if (ticket == null) return NotFound();

            var comments = await _context.TicketComments
                .Include(c => c.CreatedByUser)
                .Where(c => c.TicketId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.jpeg");

            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(QuestPDF.Helpers.PageSizes.A4);

                    // 🔹 Header: Logo + Title
                    page.Header().Column(header =>
                    {
                        if (System.IO.File.Exists(logoPath))
                        {
                            header.Item().AlignCenter().Element(container =>
                            {
                                container
                                    .Height(80)
                                    .Width(80)
                                    .Image(logoPath);
                            });
                        }

                        header.Item().AlignCenter()
                            .Text($"Ticket Report - #{ticket.TicketId}")
                            .FontSize(20).Bold().FontColor("#2c3e50");
                    });

                    // 🔹 Ticket Details Section
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Item().Background("#f0f0f0").Padding(10).Column(details =>
                        {
                            details.Item().Text($"Title: {ticket.Title}").FontSize(14).Bold();
                            details.Item().Text($"Description: {ticket.Description}").FontSize(12);
                            details.Item().Text($"Status: {ticket.Status}").FontSize(12);
                            details.Item().Text($"Assigned To: {ticket.AssignedToUser?.UserName ?? "Unassigned"}").FontSize(12);
                            details.Item().Text($"Created By: {ticket.CreatedByUser?.UserName}").FontSize(12);
                            details.Item().Text($"Created At: {ticket.CreatedAt:g}").FontSize(12);
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor("#bdc3c7");

                        // 🔹 Comments Section with Table
                        col.Item().Element(e =>
                        {
                            e.PaddingBottom(5).Text("Comments")
                                .FontSize(16).Bold().FontColor("#2c3e50").Underline();
                        });


                        col.Item().Table(table =>
                        {
                            // Columns
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(120); // Comment By
                                columns.RelativeColumn();    // Comment
                                columns.ConstantColumn(120); // Date
                            });

                            // Header Row
                            table.Header(header =>
                            {
                                header.Cell().Background("#2c3e50").Padding(5).Text("Comment By").FontColor("#ffffff").Bold();
                                header.Cell().Background("#2c3e50").Padding(5).Text("Comment").FontColor("#ffffff").Bold();
                                header.Cell().Background("#2c3e50").Padding(5).Text("Date").FontColor("#ffffff").Bold();
                            });

                            // Data Rows
                            foreach (var c in comments)
                            {
                                table.Cell().Background("#ecf0f1").Padding(5)
                                    .Text(c.CreatedByUser?.UserName ?? "Unknown");
                                table.Cell().Background("#ecf0f1").Padding(5)
                                    .Text(c.Comment);
                                table.Cell().Background("#ecf0f1").Padding(5)
                                    .Text(c.CreatedAt.ToLocalTime().ToString("g"));
                            }
                        });
                    });

                    // 🔹 Footer
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on ").FontSize(10);
                        x.Span($"{DateTime.Now:g}").FontSize(10).Bold();
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Ticket_{ticket.TicketId}.pdf");
        }




        // ---------- Helpers ----------
        private async Task AddHistory(
             int? ticketId,
             string userId,
             string action,
             CancellationToken ct,
             string? oldValue = null,
             string? newValue = null)
                {
                    var history = new TicketHistory
                    {
                        TicketId = ticketId ?? 0,
                        Action = action,
                        OldValue = oldValue,
                        NewValue = newValue,
                        ChangedByUserId = userId,
                        ChangedAt = DateTime.UtcNow
                    };

                    _context.TicketHistories.Add(history);
                    await _context.SaveChangesAsync(ct);
        }



        private async Task<string> SaveFileToUploadsAsync(IFormFile file, CancellationToken ct)
        {
            if (file.Length <= 0) throw new InvalidOperationException("Empty file.");
            if (file.Length > _maxBytes) throw new InvalidOperationException("File too large (max 5 MB).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExts.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' not allowed.");

            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsRoot = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsRoot, fileName);

            using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(fs, ct);

            return $"/uploads/{fileName}";
        }

        private Task DeleteFromUploadsAsync(string? storedPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedPath)) return Task.CompletedTask;
                var fileName = Path.GetFileName(storedPath);
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsRoot = Path.Combine(webRoot, "uploads");
                var fullPath = Path.Combine(uploadsRoot, fileName);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch { }
            return Task.CompletedTask;
        }
    }
}

ResolveIt - Ticket Management System

A role-based ASP.NET Core MVC ticket management system with Entity Framework Core (Code-First) and ASP.NET Identity.
Supports multiple roles: Admin, Help Desk Team, Engineering Team, Employees with dashboards, ticketing, comments, and history tracking.

Authentication & Authorization (ASP.NET Identity)

Role-based Dashboards:
      Admin → Manage all tickets, users, and system
      Help Desk Team → View/assign/unassign tickets, triage
      Engineering Team → View tickets assigned to them, update status
      Employees → Raise tickets, track progress
      
Ticket Management
      Create, Edit Status, Assign, Delete
      Auto-assignment to Help Desk 
      
Comments & History Tracking
File Uploads (PDF, DOCX, Images, etc. with validation)
Responsive Bootstrap UI
Notifications & Popups (toast + modals)

📦 Prerequisites

Ensure you have installed:
  .NET SDK 8.0+
  SQL Server or LocalDB
  Visual Studio 2022 / VS Code
  EF Core CLI

**Setup Guide**
1. Clone the Repository
   git clone https://github.com/yourusername/ITO_TicketManagementSystem.git
   cd ITO_TicketManagementSystem
2. Configure Database
   Edit appsettings.json and set your SQL Server connection string:
   "ConnectionStrings": {
   "DefaulConnectionString": "Server=.;Database=Ticket_Management;Trusted_Connection=True;TrustServerCertificate=True;"
   }
3. Run Code-First Migrations
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   If models change later:
   dotnet ef migrations add AddTicketHistory
   dotnet ef database update
4. Seed Roles & Users
 Roles required: Admin, Help Desk Team, Engineering Team, Employees
 Example:
   await roleManager.CreateAsync(new IdentityRole("Admin"));
   Add at least one user for each role.
5. Run the Application
   dotnet run
   Navigate to: https://localhost:7032
   
**Default Dummy Users:**
      Role                     Username                         Password
      Admin                 admin@gmail.com                     Admin@123
      Help Desk Team        HelpDeskTeam1@gmail.com             HelpDesk@123
      Help Desk Team        HelpDeskTeam2@gmail.com             HelpDesk@123
      Engineering Team      EngineeringTeam1@gmail.com          Engineering@123
      Engineering Team      EngineeringTeam2@gmail.com          Engineering@123
      Engineering Team      EngineeringTeam3@gmail.com          Engineering@123
      Employee              employee1@gmail.com                 Employee@123
      Employee              employee2@gmail.com                 Employee@123
      Employee              employee3@gmail.com                 Employee@123
      Employee              employee4@gmail.com                 Employee@123
      
Development Notes:
- Code-First: Models drive schema → run migrations after changes.
- File Uploads: Stored in /wwwroot/uploads/, max size 5 MB.
- History Tracking: Every change (status, assignment, comments) is logged.
- Notifications: Toasts appear on ticket actions

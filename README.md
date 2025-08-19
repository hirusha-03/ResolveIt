# 🛠️ ResolveIt - Ticket Management System

A **role-based ASP.NET Core MVC ticket management system** built with **Entity Framework Core (Code-First)** and **ASP.NET Identity**.  
Supports multiple roles with dedicated dashboards, ticketing, comments, and history tracking.

---

## ✨ Features

### 🔐 Authentication & Authorization
- Secure login & role management with ASP.NET Identity  
- Multiple roles supported:
  - **Admin** → Manage all tickets, users, and system  
  - **Help Desk Team** → View / assign / unassign tickets, triage  
  - **Engineering Team** → View assigned tickets, update status  
  - **Employees** → Raise tickets, track progress  

### 🎫 Ticket Management
- Create, Edit, Assign, Update Status, Delete  
- Auto-assignment to Help Desk Team  

### 💬 Comments & History
- Add comments to tickets  
- Full ticket history tracking  

### 📂 File Uploads
- Supports **PDF, DOCX, Images, TXT**  
- File validation (extensions + size limit)  

### 🎨 UI & UX
- Responsive **Bootstrap-based interface**  
- Role-based dashboards  
- Notifications & Popups (toasts + modals)  

---


## 📦 Prerequisites
Make sure you have the following installed:
- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- [SQL Server or LocalDB](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)  
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) **or** [Visual Studio Code](https://code.visualstudio.com/)  
- [Entity Framework Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)  

---

## ⚙️ Setup Guide

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/hirusha-03/ResolveIt.git
cd ITO_TicketManagementSystem
2️⃣ Configure Database

Edit appsettings.json and set your SQL Server connection string:

"ConnectionStrings": {
  "DefaulConnectionString": "Server=.;Database=Ticket_Management;Trusted_Connection=True;TrustServerCertificate=True;"
}

3️⃣ Run Code-First Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update


If models change later:

dotnet ef migrations add AddTicketHistory
dotnet ef database update

4️⃣ Seed Roles & Users

Required Roles:

Admin

Help Desk Team

Engineering Team

Employees

Example:

await roleManager.CreateAsync(new IdentityRole("Admin"));


➡️ Add at least one user per role.

5️⃣ Run the Application
dotnet run


Then open in browser:
👉 https://localhost:7032
   
## 👤 Default Dummy Users

| Role             | Username                   | Password        |
|------------------|---------------------------|-----------------|
| Admin            | admin@gmail.com           | Admin@123       |
| Help Desk Team   | HelpDeskTeam1@gmail.com   | HelpDesk@123    |
| Help Desk Team   | HelpDeskTeam2@gmail.com   | HelpDesk@123    |
| Engineering Team | EngineeringTeam1@gmail.com | Engineering@123 |
| Engineering Team | EngineeringTeam2@gmail.com | Engineering@123 |
| Engineering Team | EngineeringTeam3@gmail.com | Engineering@123 |
| Employee         | employee1@gmail.com       | Employee@123    |
| Employee         | employee2@gmail.com       | Employee@123    |
| Employee         | employee3@gmail.com       | Employee@123    |
| Employee         | employee4@gmail.com       | Employee@123    |

      
Development Notes:
- Code-First: Models drive schema → run migrations after changes.
- File Uploads: Stored in /wwwroot/uploads/, max size 5 MB.
- History Tracking: Every change (status, assignment, comments) is logged.
- Notifications: Toasts appear on ticket actions

using ITO_TicketManagementSystem.Data;
using ITO_TicketManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Services
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<MyAppContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaulConnectionString"))); // ensure key matches appsettings.json

        builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false; // dev-friendly
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<MyAppContext>();

        var app = builder.Build();

        QuestPDF.Settings.License = LicenseType.Community;
        // Pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();

        // Migrate + seed roles and users (Admin + two Employees)
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var db = services.GetRequiredService<MyAppContext>();
            await db.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Admin", "Help Desk Team", "Engineering Team", "Employees" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            await EnsureUserInRole(userManager, "admin@gmail.com", "Admin@123", "Admin");
           
            await EnsureUserInRole(userManager, "employee1@gmail.com", "Employee@123", "Employees");
            await EnsureUserInRole(userManager, "employee2@gmail.com", "Employee@123", "Employees");
            await EnsureUserInRole(userManager, "employee3@gmail.com", "Employee@123", "Employees");
            await EnsureUserInRole(userManager, "employee4@gmail.com", "Employee@123", "Employees");
            
            await EnsureUserInRole(userManager, "HelpDeskTeam1@gmail.com", "HelpDesk@123", "Help Desk Team");
            await EnsureUserInRole(userManager, "HelpDeskTeam2@gmail.com", "HelpDesk@123", "Help Desk Team");
                        
            await EnsureUserInRole(userManager, "EngineeringTeam1@gmail.com", "Engineering@123", "Engineering Team");
            await EnsureUserInRole(userManager, "EngineeringTeam2@gmail.com", "Engineering@123", "Engineering Team");
            await EnsureUserInRole(userManager, "EngineeringTeam3@gmail.com", "Engineering@123", "Engineering Team");
            

        }

        app.Run();
    }

    private static async Task EnsureUserInRole(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true // avoids confirm step
            };
            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded) return; // you can log errors here if needed
        }
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}

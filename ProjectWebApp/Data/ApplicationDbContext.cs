using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ProjectWebApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    //Added for DMIT2018
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseSeeding((context, _) =>
        {
            var roleManager = context.GetService<RoleManager<IdentityRole>>();
            var userManager = context.GetService<UserManager<ApplicationUser>>();
            SeedRolesAsync(roleManager).Wait();
            SeedAdminUserAsync(userManager).Wait();
            SeedTestUsersAsync(userManager).Wait();
        });

    static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roleNames = new[] { "Mechanic", "Salesperson", "Store Staff", "Sales Manager", "Shop Manager", "Parts Manager" };

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
    //Method to create default admin user with all roles
    static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.FindByIdAsync("9371edbd-8227-4e48-885e-4e3140f12959");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "9371edbd-8227-4e48-885e-4e3140f12959",
                UserName = "admin@ebikes.com", 
                Email = "admin@ebikes.com", 
                Address = "123 Example Road", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5T5T5",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Admin",
                LastName = "User",
                SocialInsuranceNumber = "123456789"
            };
            await userManager.CreateAsync(user, "Admin@123");

            // Assign All Roles to Admin User
            await userManager.AddToRoleAsync(user, "Mechanic");
            await userManager.AddToRoleAsync(user, "Salesperson");
            await userManager.AddToRoleAsync(user, "Store Staff");
            await userManager.AddToRoleAsync(user, "Sales Manager");
            await userManager.AddToRoleAsync(user, "Shop Manager");
            await userManager.AddToRoleAsync(user, "Parts Manager");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
    }
    static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Create First Mechanic User
        var user = await userManager.FindByIdAsync("20e005ab-cf07-48bb-98bc-f55cb91f59fd");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "20e005ab-cf07-48bb-98bc-f55cb91f59fd",
                UserName = "mechanic@ebikes.com", 
                Email = "mechanic@ebikes.com", 
                Address = "3215 - 66 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J1X1",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Nole",
                LastName = "Body",
                SocialInsuranceNumber = "213242526"
            };
            await userManager.CreateAsync(user, "Mechanic@123");

            await userManager.AddToRoleAsync(user, "Mechanic");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Second Mechanic User
        user = await userManager.FindByIdAsync("dc811d97-3686-4566-91dd-214a05b47ee4");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "dc811d97-3686-4566-91dd-214a05b47ee4",
                UserName = "mechanic2@ebikes.com", 
                Email = "mechanic2@ebikes.com", 
                Address = "12345 - 67 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X2",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Ken",
                LastName = "Fixit",
                SocialInsuranceNumber = "333444555"
            };
            await userManager.CreateAsync(user, "Mechanic@123");

            await userManager.AddToRoleAsync(user, "Mechanic");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create First Sales User
        user = await userManager.FindByIdAsync("d3ea9e87-5d68-4bfc-afb3-9e527a919988");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "d3ea9e87-5d68-4bfc-afb3-9e527a919988",
                UserName = "sales@ebikes.com", 
                Email = "sales@ebikes.com", 
                Address = "6776 - 55 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X8",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Max",
                LastName = "Smart",
                SocialInsuranceNumber = "432432432"
            };
            await userManager.CreateAsync(user, "Sales@123");

            await userManager.AddToRoleAsync(user, "Salesperson");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Second Sales User
        user = await userManager.FindByIdAsync("1cb0fd7e-510a-4cb4-a23b-c20339846c68");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "1cb0fd7e-510a-4cb4-a23b-c20339846c68",
                UserName = "sales2@ebikes.com", 
                Email = "sales2@ebikes.com", 
                Address = "7887 - 55 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X9",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Sheila",
                LastName = "Seller",
                SocialInsuranceNumber = "543543543"
            };
            await userManager.CreateAsync(user, "Sales@123");

            await userManager.AddToRoleAsync(user, "Salesperson");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create First Store Staff User
        user = await userManager.FindByIdAsync("a5b7e34b-83dd-4a57-98aa-a02dc958debb");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "a5b7e34b-83dd-4a57-98aa-a02dc958debb",
                UserName = "staff@ebikes.com", 
                Email = "staff@ebikes.com", 
                Address = "6565 - 55 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X7",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Sadie",
                LastName = "Star",
                SocialInsuranceNumber = "432432432"
            };
            await userManager.CreateAsync(user, "Staff@123");

            await userManager.AddToRoleAsync(user, "Store Staff");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Second Store Staff User
        user = await userManager.FindByIdAsync("20da03d1-b416-4278-b79b-5da825d20c9c");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "20da03d1-b416-4278-b79b-5da825d20c9c",
                UserName = "staff2@ebikes.com", 
                Email = "staff2@ebikes.com", 
                Address = "4354 - 55 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X6",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Fred",
                LastName = "Flash",
                SocialInsuranceNumber = "678678678"
            };
            await userManager.CreateAsync(user, "Staff@123");

            await userManager.AddToRoleAsync(user, "Store Staff");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Sales Manager User
        user = await userManager.FindByIdAsync("842104df-6e15-411b-8d76-3ca29da164c4");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "842104df-6e15-411b-8d76-3ca29da164c4",
                UserName = "salesmanager@ebikes.com", 
                Email = "salesmanager@ebikes.com", 
                Address = "7887 - 55 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X9",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Shirly",
                LastName = "Awesome",
                SocialInsuranceNumber = "566435551"
            };
            await userManager.CreateAsync(user, "Sales@123");

            await userManager.AddToRoleAsync(user, "Sales Manager");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Shop Manager User
        user = await userManager.FindByIdAsync("a586b583-d1ac-4c5b-b387-9a2080e03861");
        if (user == null)
        {
            user = new ApplicationUser 
            { 
                Id = "a586b583-d1ac-4c5b-b387-9a2080e03861",
                UserName = "shop@ebikes.com", 
                Email = "shop@ebikes.com", 
                Address = "4545 - 57 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J2X4",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Brendon",
                LastName = "Brown",
                SocialInsuranceNumber = "456456456"
            };
            await userManager.CreateAsync(user, "Shop@123");

            await userManager.AddToRoleAsync(user, "Shop Manager");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
        //Create Parts Manager User
        user = await userManager.FindByIdAsync("15b6e2f5-af87-44e7-aa3e-51054d911edd");
        if (user == null)
        {
            user = new ApplicationUser 
            {
                Id = "15b6e2f5-af87-44e7-aa3e-51054d911edd",
                UserName = "parts@ebikes.com", 
                Email = "parts@ebikes.com", 
                Address = "12345 - 67 St", 
                City = "Edmonton",
                Province = "AB",
                PostalCode = "T5J1X1",
                PhoneNumber = "780-555-5555",
                Textable = true,
                FirstName = "Willie",
                LastName = "Work",
                SocialInsuranceNumber = "444555666"
            };
            await userManager.CreateAsync(user, "Parts@123");

            await userManager.AddToRoleAsync(user, "Parts Manager");
            await userManager.ConfirmEmailAsync(user, await userManager.GenerateEmailConfirmationTokenAsync(user));
        }
    }
}

using BpmnWorkflow.Application.Interfaces;
using BpmnWorkflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BpmnWorkflow.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            // Ensure database is created and migrations are applied
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                var adminRole = new Role { Name = "Admin", Description = "System Administrator", CanView = true, CanCreate = true, CanEdit = true, CanDelete = true };
                var userRole = new Role { Name = "User", Description = "Default User", CanView = true, CanCreate = true, CanEdit = true };
                
                context.Roles.AddRange(adminRole, userRole);
                await context.SaveChangesAsync();
            }

            // 2. Seed Admin User
            if (!await context.Users.AnyAsync())
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    FirstName = "System",
                    LastName = "Admin",
                    PasswordHash = passwordService.HashPassword("admin123"),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            // 3. Seed Camunda Environment from appsettings.json if none exists
            if (!await context.CamundaEnvironments.AnyAsync())
            {
                var camundaConfig = configuration.GetSection("Camunda");
                var baseUrl = camundaConfig["BaseUrl"] ?? "http://localhost:8081/engine-rest/";
                
                var defaultEnv = new CamundaEnvironment
                {
                    Name = "Local Camunda",
                    BaseUrl = baseUrl,
                    Username = camundaConfig["Username"] ?? "demo",
                    Password = camundaConfig["Password"] ?? "demo",
                    Description = "Default environment from configuration",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.CamundaEnvironments.Add(defaultEnv);
                await context.SaveChangesAsync();
            }
        }
    }
}

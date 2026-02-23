using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Infrastructure.Persistence;

/// <summary>
/// Database seeder for initial data
/// </summary>
public static class DbSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        string[] roles = { "ADMIN", "TEACHER", "STUDENT" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        // Create default admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@scholarflow.com",
            Email = "admin@scholarflow.com",
            EmailConfirmed = true
        };

        var adminExists = await userManager.FindByEmailAsync(adminUser.Email);
        if (adminExists == null)
        {
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "ADMIN");
            }
        }

        // Create sample teacher user
        var teacherUser = new ApplicationUser
        {
            UserName = "teacher@scholarflow.com",
            Email = "teacher@scholarflow.com",
            EmailConfirmed = true
        };

        var teacherExists = await userManager.FindByEmailAsync(teacherUser.Email);
        if (teacherExists == null)
        {
            var result = await userManager.CreateAsync(teacherUser, "Teacher@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(teacherUser, "TEACHER");
            }
        }

        // Create sample student user
        var studentUser = new ApplicationUser
        {
            UserName = "student@scholarflow.com",
            Email = "student@scholarflow.com",
            EmailConfirmed = true
        };

        var studentExists = await userManager.FindByEmailAsync(studentUser.Email);
        if (studentExists == null)
        {
            var result = await userManager.CreateAsync(studentUser, "Student@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "STUDENT");
            }
        }
    }

    public static async Task SeedAllAsync(IServiceProvider serviceProvider)
    {
        await SeedRolesAsync(serviceProvider);
        await SeedUsersAsync(serviceProvider);
    }
}

using Mechanic.Domain.Constants;
using Mechanic.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Mechanic.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            // Default roles
            await SeedRolesAsync();
            
            // Default users
            await SeedUsersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var administrator = new ApplicationUser { UserName = "administrator", Email = "administrator@gmail.com" };
        
        if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        {
            await _userManager.CreateAsync(administrator, "AdministratorPassword1!");
            if (!string.IsNullOrWhiteSpace(Roles.Administrator))
            {
                await _userManager.AddToRolesAsync(administrator, new [] { Roles.Administrator });
            }
        }
        
        var mechanic = new ApplicationUser { UserName = "mechanic", Email = "mechanic@gmail.com" };
        
        if (_userManager.Users.All(u => u.UserName != mechanic.UserName))
        {
            await _userManager.CreateAsync(mechanic, "MechanicPassword1!");
            if (!string.IsNullOrWhiteSpace(Roles.Mechanic))
            {
                await _userManager.AddToRolesAsync(mechanic, new [] { Roles.Mechanic });
            }
        }
        
        var client = new ApplicationUser { UserName = "client", Email = "client@gmail.com" };

        if (_userManager.Users.All(u => u.UserName != client.UserName))
        {
            await _userManager.CreateAsync(client, "ClientPassword1!");
            if (!string.IsNullOrWhiteSpace(Roles.Client))
            {
                await _userManager.AddToRolesAsync(client, new [] { Roles.Client });
            }
        }
    }

    private async Task SeedRolesAsync()
    {
        var administratorRole = new IdentityRole(Roles.Administrator);

        if (_roleManager.Roles.All(r => r.Name != administratorRole.Name))
        {
            await _roleManager.CreateAsync(administratorRole);
        }
        
        var mechanicRole = new IdentityRole(Roles.Mechanic);

        if (_roleManager.Roles.All(r => r.Name != mechanicRole.Name))
        {
            await _roleManager.CreateAsync(mechanicRole);
        }

        var clientRole = new IdentityRole(Roles.Client);

        if (_roleManager.Roles.All(r => r.Name != clientRole.Name))
        {
            await _roleManager.CreateAsync(clientRole);
        }
    }
}

using LawyerProject.Domain.Constants;
using LawyerProject.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LawyerProject.Infrastructure.Data;
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
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<User> userManager, RoleManager<Role> roleManager)
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
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles
        var roleNames = new[]
        {
            Roles.Administrator,
            Roles.RegionAdmin,
            Roles.Lawyer,
            //Roles.LawyerAssistant,
            Roles.Secretary,
            Roles.Client,
            Roles.Express,
            Roles.Litigant
        };

        foreach (var roleName in roleNames)
        {
            if (_roleManager.Roles.All(r => r.Name != roleName))
            {
                var role = new Role
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                };
                await _roleManager.CreateAsync(role);
            }
        }
        // Default users
        //var administrator = new User
        //{
        //    FirstName = "admin",
        //    LastName = "admin",
        //    NationalCode = "0",
        //    UserName = "administrator@localhost",
        //    Email = "administrator@localhost"
        //};

        //if (_userManager.Users.All(u => u.UserName != administrator.UserName))
        //{
        //    await _userManager.CreateAsync(administrator, "Administrator1!");
        //    if (!string.IsNullOrWhiteSpace(administratorRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });
        //    }
        //}
        ////اضافه کردن نقش های و کاربران پیش فرض برای تست
        //#region
        //// Default region
        //var defaultRegion = _context.Regions.FirstOrDefault(r => r.Name == "Default Region");
        //if (defaultRegion == null)
        //{
        //    defaultRegion = new Region
        //    {
        //        Name = "Default Region",
        //        Address = "Default Address",
        //        PhoneNumber = "0000000000",
        //        Email = "default@region.com",
        //        DomainUrl = "http://localhost:81",
        //        IsActive = true,
        //        CreatedBy = "System"
        //    };

        //    _context.Regions.Add(defaultRegion);
        //    await _context.SaveChangesAsync();
        //}
        //var LawyerRole = new Role
        //{
        //    Id = Guid.NewGuid().ToString(),
        //    Name = Roles.Lawyer,
        //    NormalizedName = Roles.Lawyer.ToUpper()
        //};
        //if (_roleManager.Roles.All(r => r.Name != LawyerRole.Name)) await _roleManager.CreateAsync(LawyerRole);
        //var ClientRole = new Role
        //{
        //    Id = Guid.NewGuid().ToString(),
        //    Name = Roles.Client,
        //    NormalizedName = Roles.Client.ToUpper()
        //};
        //if (_roleManager.Roles.All(r => r.Name != ClientRole.Name)) await _roleManager.CreateAsync(ClientRole);
        //var ExpressRole = new Role
        //{
        //    Id = Guid.NewGuid().ToString(),
        //    Name = Roles.Express,
        //    NormalizedName = Roles.Express.ToUpper()
        //};
        //if (_roleManager.Roles.All(r => r.Name != ExpressRole.Name)) await _roleManager.CreateAsync(ExpressRole);
        //var LitigantRole = new Role
        //{
        //    Id = Guid.NewGuid().ToString(),
        //    Name = Roles.Litigant,
        //    NormalizedName = Roles.Litigant.ToUpper()
        //};
        //if (_roleManager.Roles.All(r => r.Name != LitigantRole.Name)) await _roleManager.CreateAsync(LitigantRole);

        //var lawyerTest = new User
        //{
        //    FirstName = "lawyerTest",
        //    LastName = "lawyerTest",
        //    NationalCode = "0",
        //    UserName = "lawyerTest@localhost",
        //    Email = "lawyerTest@localhost",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != lawyerTest.UserName))
        //{
        //    await _userManager.CreateAsync(lawyerTest, "lawyerTest!0");
        //    if (!string.IsNullOrWhiteSpace(LawyerRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(lawyerTest, new[] { LawyerRole.Name });
        //    }
        //}
        //var najafi = new User
        //{
        //    FirstName = "علی اصغر",
        //    LastName = "نجفی",
        //    NationalCode = "0",
        //    UserName = "AliAsghar@Najafi",
        //    Email = "AliAsghar@Najafi",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != najafi.UserName))
        //{
        //    await _userManager.CreateAsync(najafi, "Najafi123.");
        //    if (!string.IsNullOrWhiteSpace(LawyerRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(najafi, new[] { LawyerRole.Name });
        //    }
        //}
        //var ClientTest = new User
        //{
        //    FirstName = "ClientTest",
        //    LastName = "ClientTest",
        //    NationalCode = "0",
        //    UserName = "ClientTest@localhost",
        //    Email = "ClientTest@localhost",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != ClientTest.UserName))
        //{
        //    await _userManager.CreateAsync(ClientTest, "ClientTest!0");
        //    if (!string.IsNullOrWhiteSpace(ClientRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(ClientTest, new[] { ClientRole.Name });
        //    }
        //}
        //var ClientTest2 = new User
        //{
        //    FirstName = "ClientTest2",
        //    LastName = "ClientTest2",
        //    NationalCode = "0",
        //    UserName = "ClientTest2@localhost",
        //    Email = "ClientTest2@localhost",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != ClientTest2.UserName))
        //{
        //    await _userManager.CreateAsync(ClientTest2, "ClientTest!0");
        //    if (!string.IsNullOrWhiteSpace(ClientRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(ClientTest2, new[] { ClientRole.Name });
        //    }
        //}
        //var ExpressTest = new User
        //{
        //    FirstName = "ExpressTest",
        //    LastName = "ExpressTest",
        //    NationalCode = "0",
        //    UserName = "ExpressTest@localhost",
        //    Email = "ExpressTest@localhost",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != ExpressTest.UserName))
        //{
        //    await _userManager.CreateAsync(ExpressTest, "ExpressTest!0");
        //    if (!string.IsNullOrWhiteSpace(ExpressRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(ExpressTest, new[] { ExpressRole.Name });
        //    }
        //}
        //var ExpressTest2 = new User
        //{
        //    FirstName = "ExpressTest2",
        //    LastName = "ExpressTest2",
        //    NationalCode = "0",
        //    UserName = "ExpressTest2@localhost",
        //    Email = "ExpressTest2@localhost",
        //    IsActive = true,
        //    EmailConfirmed = true
        //};
        //if (_userManager.Users.All(u => u.UserName != ExpressTest2.UserName))
        //{
        //    await _userManager.CreateAsync(ExpressTest2, "ExpressTest!0");
        //    if (!string.IsNullOrWhiteSpace(ExpressRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(ExpressTest2, new[] { ExpressRole.Name });
        //    }
        //}
        //var LitigantTest1 = new User
        //{
        //    FirstName = "محمد",
        //    LastName = "سعیدی",
        //    NationalCode = "0",
        //    UserName = "LitigantTest1@localhost",
        //    Email = "LitigantTest1@localhost",
        //    IsActive = false,
        //    EmailConfirmed = false
        //};
        //if (_userManager.Users.All(u => u.UserName != LitigantTest1.UserName))
        //{
        //    await _userManager.CreateAsync(LitigantTest1, "LitigantTest!0");
        //    if (!string.IsNullOrWhiteSpace(LitigantRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(LitigantTest1, new[] { LitigantRole.Name });
        //    }

        //}
        //var LitigantTest2 = new User
        //{
        //    FirstName = "رضا",
        //    LastName = "رضایی",
        //    NationalCode = "0",
        //    UserName = "LitigantTest2@localhost",
        //    Email = "LitigantTest2@localhost",
        //    IsActive = false,
        //    EmailConfirmed = false
        //};
        //if (_userManager.Users.All(u => u.UserName != LitigantTest2.UserName))
        //{
        //    await _userManager.CreateAsync(LitigantTest2, "LitigantTest!0");
        //    if (!string.IsNullOrWhiteSpace(LitigantRole.Name))
        //    {
        //        await _userManager.AddToRolesAsync(LitigantTest2, new[] { LitigantRole.Name });
        //    }
        //}
        //#endregion

        ////اضافه کردن کاربران تست به ناحیه
        //#region
        //var adminInDb = await _userManager.FindByNameAsync(administrator.UserName);
        //if (adminInDb == null) throw new InvalidOperationException("Administrator user was not created successfully.");
        //// Add to default region
        //var regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == adminInDb.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = adminInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var adminRoleInDb = await _roleManager.FindByNameAsync(administratorRole.Name);
        //if (adminRoleInDb == null) throw new InvalidOperationException("adminRoleInDb was not created successfully.");
        //var roleUserExists = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == adminInDb.Id && ru.RoleId == adminRoleInDb.Id);
        //if (!roleUserExists)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = adminRoleInDb.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = adminInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var clientInDb = await _userManager.FindByNameAsync(ClientTest.UserName);
        //if (clientInDb == null) throw new InvalidOperationException("ClientTest user was not created successfully.");
        //// Add to default region
        //regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == clientInDb.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = clientInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var clientRoleInDb = await _roleManager.FindByNameAsync(ClientRole.Name);
        //if (clientRoleInDb == null) throw new InvalidOperationException("clientRoleInDb was not created successfully.");
        //roleUserExists = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == clientInDb.Id && ru.RoleId == clientRoleInDb.Id);
        //if (!roleUserExists)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = clientRoleInDb.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = clientInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var clientInDb2 = await _userManager.FindByNameAsync(ClientTest2.UserName);
        //if (clientInDb2 == null) throw new InvalidOperationException("ClientTest2 user was not created successfully.");
        //// Add to default region
        //regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == clientInDb2.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = clientInDb2.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var clientRoleInDb2 = await _roleManager.FindByNameAsync(ClientRole.Name);
        //if (clientRoleInDb2 == null) throw new InvalidOperationException("clientRoleInDb was not created successfully.");
        //roleUserExists = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == clientInDb2.Id && ru.RoleId == clientRoleInDb2.Id);
        //if (!roleUserExists)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = clientRoleInDb2.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = clientInDb2.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var lawyerInDb = await _userManager.FindByNameAsync(lawyerTest.UserName);
        //if (lawyerInDb == null) throw new InvalidOperationException("lawyerTest user was not created successfully.");
        //regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == lawyerInDb.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = lawyerInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var lawyerRoleInDb = await _roleManager.FindByNameAsync(LawyerRole.Name);
        //if (lawyerRoleInDb == null) throw new InvalidOperationException("lawyerRoleInDb was not created successfully.");
        //roleUserExists = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == lawyerInDb.Id && ru.RoleId == lawyerRoleInDb.Id);
        //if (!roleUserExists)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = lawyerRoleInDb.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = lawyerInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}

        //var expressInDb = await _userManager.FindByNameAsync(ExpressTest.UserName);
        //if (expressInDb == null) throw new InvalidOperationException("ExpressTest user was not created successfully.");
        //regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == expressInDb.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = expressInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var expressRoleInDb = await _roleManager.FindByNameAsync(ExpressRole.Name);
        //if (expressRoleInDb == null) throw new InvalidOperationException("expressRoleInDb was not created successfully.");
        //var expressRoleUserExists = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == expressInDb.Id && ru.RoleId == expressRoleInDb.Id);
        //if (!expressRoleUserExists)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = expressRoleInDb.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = expressInDb.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var expressInDb2 = await _userManager.FindByNameAsync(ExpressTest2.UserName);
        //if (expressInDb2 == null) throw new InvalidOperationException("ExpressTest2 user was not created successfully.");
        //regionUserExists = _context.RegionsUsers.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == expressInDb2.Id);
        //if (!regionUserExists)
        //{
        //    _context.RegionsUsers.Add(new RegionsUser
        //    {
        //        RegionId = defaultRegion.Id,
        //        UserId = expressInDb2.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}
        //var expressRoleInDb2 = await _roleManager.FindByNameAsync(ExpressRole.Name);
        //if (expressRoleInDb2 == null) throw new InvalidOperationException("expressRoleInDb was not created successfully.");
        //var expressRoleUserExists2 = _context.UsersRoles.Any(ru => ru.RegionId == defaultRegion.Id && ru.UserId == expressInDb2.Id && ru.RoleId == expressRoleInDb2.Id);
        //if (!expressRoleUserExists2)
        //{
        //    _context.UserRoles.Add(new UsersRole
        //    {
        //        RoleId = expressRoleInDb2.Id,
        //        RegionId = defaultRegion.Id,
        //        UserId = expressInDb2.Id,
        //        CreatedBy = "System"
        //    });
        //    await _context.SaveChangesAsync();
        //}

        ////دو کاربر تست طرف دعوی به صورت دستی به جداول واسط ریجن یوزر و رول یوزر در سرور اضافه شدند
        //#endregion

        // Default data
        // Seed, if necessary
        //if (!_context.TodoLists.Any())
        //{
        //    _context.TodoLists.Add(new TodoList
        //    {
        //        Title = "Todo List",
        //        Items =
        //        {
        //            new TodoItem { Title = "Make a todo list 📃" },
        //            new TodoItem { Title = "Check off the first item ✅" },
        //            new TodoItem { Title = "Realise you've already done two things on the list! 🤯"},
        //            new TodoItem { Title = "Reward yourself with a nice, long nap 🏆" },
        //        }
        //    });

        //    await _context.SaveChangesAsync();
        //}
    }

}

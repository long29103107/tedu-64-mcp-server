using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.ValueObjects;
using OrderManagement.Infrastructure.Identity;

namespace OrderManagement.Infrastructure.Persistence.Seeding;

public class DatabaseSeeder(
    ApplicationDbContext context,
    UserManager<AppUser> userManager,
    ILogger<DatabaseSeeder> logger)
{
    private static readonly Guid AdminUserId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    private const string AdminEmail = "admin@orderms.local";
    private const string AdminPassword = "Admin@123456";
    private const string AdminFullName = "System Administrator";

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await HasAnySeedDataAsync(ct))
        {
            logger.LogInformation("Database already contains seed data. Skipping database seeding.");
            return;
        }

        await SeedRolesAsync(ct);
        await SeedAdminUserAsync(ct);
        await SeedDevelopmentCustomersAsync(ct);
        await SeedDevelopmentOrdersAsync(ct);   
    }

    private async Task<bool> HasAnySeedDataAsync(CancellationToken ct)
    {
        return await context.Roles.AnyAsync(ct)
            || await context.Set<User>().AnyAsync(ct)
            || await context.Customers.AnyAsync(ct)
            || await context.Products.AnyAsync(ct)
            || await context.Orders.AnyAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (await context.Roles.AnyAsync(ct))
            return;

        var roles = new[]
        {
            new AppRole { Id = Guid.Parse("25313ed6-c08b-4012-b3e1-3b841f77a939"), Name = "Admin", NormalizedName = "ADMIN" },
            new AppRole { Id = Guid.Parse("1cf28b17-4cae-4710-95b8-2157e5ba4ccc"), Name = "Manager", NormalizedName = "MANAGER" },
            new AppRole { Id = Guid.Parse("ce7ded3e-a11d-408d-b5aa-e9caea44d28d"), Name = "Staff", NormalizedName = "STAFF" }
        };

        await context.Roles.AddRangeAsync(roles, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seeded roles.");
    }

    private async Task SeedAdminUserAsync(CancellationToken ct)
    {
        var existing = await userManager.FindByEmailAsync(AdminEmail);
        if (existing is not null)
        {
            logger.LogInformation("Admin user already exists. Skipping admin seeding.");
            return;
        }

        var appUser = new AppUser
        {
            Id = AdminUserId,
            UserName = AdminEmail,
            Email = AdminEmail,
            FullName = AdminFullName,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(appUser, AdminPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Could not create admin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(appUser, "Admin");

        var domainUser = User.Create(AdminUserId, AdminFullName, AdminEmail);
        await context.Set<User>().AddAsync(domainUser, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seeded admin user.");
    }

    private async Task SeedDevelopmentCustomersAsync(CancellationToken ct)
    {
        if (await context.Customers.AnyAsync(ct))
            return;

        var customers = new List<Customer>();
        var firstNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Vo", "Dang", "Bui", "Do", "Vu" };
        var lastNames = new[] { "Van A", "Thi B", "Van C", "Thi D", "Van E", "Thi F", "Van G", "Thi H", "Van I", "Thi K" };
        var cities = new[]
        {
            ("Ho Chi Minh", "HCM", "70000"),
            ("Ha Noi", "HN", "10000"),
            ("Da Nang", "DN", "50000"),
            ("Can Tho", "CT", "90000"),
            ("Hai Phong", "HP", "18000")
        };

        for (var i = 0; i < 10; i++)
        {
            var city = cities[i % cities.Length];
            var address = Address.Create(
                street: $"{Random.Shared.Next(1, 999)} Nguyen Hue Street",
                city: city.Item1,
                province: city.Item2,
                country: "VN",
                postalCode: city.Item3);

            var customerResult = Customer.Create(
                firstName: firstNames[i],
                lastName: lastNames[i],
                email: $"customer{i + 1}@example.com",
                phoneNumber: $"0{Random.Shared.Next(900000000, 999999999)}",
                billingAddress: address);

            if (customerResult.IsSuccess)
            {
                customers.Add(customerResult.Value);
            }
        }

        await context.Customers.AddRangeAsync(customers, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seeded development customers.");
    }

    private async Task SeedDevelopmentOrdersAsync(CancellationToken ct)
    {
        if (await context.Orders.AnyAsync(ct))
            return;

        var customers = await context.Customers
            .Select(c => new { c.Id, c.Email })
            .Take(10)
            .ToListAsync(ct);

        if (customers.Count == 0)
            return;

        if (!await context.Products.AnyAsync(ct))
        {
            var products = Enumerable.Range(1, 10).Select(i =>
                Product.Create(
                    name: $"Sample Product {i}",
                    description: $"Description for product {i}",
                    price: Money.Create(Random.Shared.Next(100, 1000) * 1000m, "VND"),
                    weightKg: Random.Shared.Next(1, 20) * 0.5m,
                    stockQuantity: Random.Shared.Next(50, 200)));

            await context.Products.AddRangeAsync(products, ct);
            await context.SaveChangesAsync(ct);
        }

        var productList = await context.Products.Take(10).ToListAsync(ct);
        if (productList.Count == 0)
            return;

        var addressData = new[]
        {
            ("123 Nguyen Hue", "Ho Chi Minh", "HCM", "VN", "70000"),
            ("456 Le Loi", "Ha Noi", "HN", "VN", "10000"),
            ("789 Tran Hung Dao", "Da Nang", "DN", "VN", "50000")
        };

        var orders = new List<Order>();
        for (var i = 1; i <= 20; i++)
        {
            var customer = customers[Random.Shared.Next(customers.Count)];
            var addressTemplate = addressData[Random.Shared.Next(addressData.Length)];
            var address = Address.Create(
                addressTemplate.Item1,
                addressTemplate.Item2,
                addressTemplate.Item3,
                addressTemplate.Item4,
                addressTemplate.Item5);

            var orderResult = Order.CreateDraft(customer.Id, address, customer.Email);
            if (!orderResult.IsSuccess)
            {
                logger.LogWarning("Could not create development order: {Error}", orderResult.Error);
                continue;
            }

            var order = orderResult.Value;
            var itemCount = Random.Shared.Next(1, 4);
            for (var j = 0; j < itemCount; j++)
            {
                var product = productList[Random.Shared.Next(productList.Count)];
                var unitPrice = Money.Create(product.Price.Amount, product.Price.Currency);

                order.AddItem(
                    productId: product.Id,
                    productName: product.Name,
                    unitPrice: unitPrice,
                    quantity: Random.Shared.Next(1, 5));
            }

            orders.Add(order);
        }

        await context.Orders.AddRangeAsync(orders, ct);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Seeded development orders.");
    }
}

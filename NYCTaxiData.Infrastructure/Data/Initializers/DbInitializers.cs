using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using NYCTaxiData.Infrastructure.Data.Contexts;

namespace NYCTaxiData.Infrastructure.Data
{
    public class DbInitializer(TaxiDbContext context) : IDbInitializer
    {
        private readonly TaxiDbContext _context = context;

        public async Task InitializeAsync()
        { 
            if (!await _context.Users1.AnyAsync())
            {
                await SeedAsync();
            }
        }

        private async Task SeedAsync()
        {
            var adminId = Guid.NewGuid();
            var userId = Guid.NewGuid() ;
            var manager1Id = Guid.NewGuid();
            var manager2Id = Guid.NewGuid();

            var users = new List<User1>
            {
                new User1 { Id = adminId, Firstname = "Admin", Lastname = "Admin", Phonenumber = "123-456-7890", Passwordhash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), Age = 40, City = "New York", Street = "Admin Street 123", Createdat = DateTime.UtcNow },
                new User1 { Id = userId, Firstname = "John", Lastname = "Smith", Phonenumber = "098-765-4321", Passwordhash = BCrypt.Net.BCrypt.HashPassword("User123!"), Age = 28, City = "New York", Street = "User Ave 456", Createdat = DateTime.UtcNow },
                new User1 { Id = manager1Id, Firstname = "Jane", Lastname = "Doe", Phonenumber = "555-123-4567", Passwordhash = BCrypt.Net.BCrypt.HashPassword("Manager123!"), Age = 35, City = "New York", Street = "Manager Blvd 789", Createdat = DateTime.UtcNow },
                new User1 { Id = manager2Id, Firstname = "Mike", Lastname = "Johnson", Phonenumber = "555-987-6543", Passwordhash = BCrypt.Net.BCrypt.HashPassword("Manager123!"), Age = 42, City = "New York", Street = "Ops Street 101", Createdat = DateTime.UtcNow }
            };

            var managers = new List<Manager>
            {
                new Manager { Id = manager1Id, Employeeid = "EMP-001", Department = "Taxi Operations" },
                new Manager { Id = manager2Id, Employeeid = "EMP-002", Department = "Fleet Management" }
            };

            // Add a Driver for testing if needed
            // var driver = new Driver { Id = someUserId, Fullname = "...", ... };

            _context.Users1.AddRange(users);
            _context.Managers.AddRange(managers);
            // _context.Drivers.Add(driver);

            await _context.SaveChangesAsync();
        }
    }
}

using APILibrary.Test.Mock.Models;
using APILibraryTest.Mocks.Models;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;

namespace APILibrary.Test.Mock
{
    public class DbContextMock : EatDbContext
    {
        public DbContextMock(DbContextOptions options) : base(options)
        {
        }

        public static DbContextMock GetDbContext(bool withData = true)
        {
            var options = new DbContextOptionsBuilder().UseInMemoryDatabase("dbtest").Options;
            var db = new DbContextMock(options);

            if (withData)
            {
                db.Pizzas.Add(new PizzaMock { Name = "Margharita", Price = 10, Topping = "Mozzarella" });
                db.Pizzas.Add(new PizzaMock { Name = "Royale", Price = 11, Topping = "Champignons" });
                db.Pizzas.Add(new PizzaMock { Name = "Quatre fromage", Price = 12, Topping = "Chèvre" });

                db.Customers.Add(new CustomerMock { FirstName = "Regis", LastName = "Grumberg", Email = "regisgb@yolo.com", Phone = "0605040302" });
                db.Customers.Add(new CustomerMock { FirstName = "Wassim", LastName = "Matougui", Email = "wsmmtg@yolo.com", Phone = "0605080302" });

                db.SaveChanges();
            }

            return db;
        }
    }
}
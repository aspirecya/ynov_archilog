using APILibrary.Test.Mock;
using APILibrary.Test.Mock.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Controllers;

namespace APILibrary.Test
{
    public class Tests
    {
        private DbContextMock _db;
        private PizzaController _PizzaController;
        private CustomerController _CustomerController;

        [SetUp]
        public void Setup()
        {
            _db = DbContextMock.GetDbContext();
            _PizzaController = new PizzaController(_db);
            _CustomerController = new CustomerController(_db);
        }

        [Test]
        public async Task TestPizzaCount()
        {
            var actionResult = await _PizzaController.GetAllAsync("", "", "", "");
            var result = actionResult.Result as ObjectResult;
            var values = ((IEnumerable<object>)(result).Value);

            Assert.AreEqual((int)System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(_db.Pizzas.Count(), values.Count());
        }


        [Test]
        public async Task TestCustomerCount()
        {
            var actionResult = await _CustomerController.GetAllAsync("", "", "", "");
            var result = actionResult.Result as ObjectResult;
            var values = ((IEnumerable<object>)(result).Value);

            Assert.AreEqual((int)System.Net.HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(_db.Customers.Count(), values.Count());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CarExchangeAPI.Dao;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CarExchangeAPI.Tests
{
    public class CarTests
    {
        private HttpClient _client;
        private CustomWebApplicationFactory _factory;
        private const string RequestUrl = "/api/Cars/";

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();

        }

        [Test]
        public async Task carController_GetById_ReturnCarModel()
        {
            var httpResponse = await _client.GetAsync(RequestUrl + 1);

            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var car = JsonConvert.DeserializeObject<CarExchangeAPI.Orchestrator.Cars.Contracts.Car>(stringResponse);
            
            Assert.AreEqual(1, car.Id);
            Assert.AreEqual(1, car.Brand);
            Assert.AreEqual(false, car.Damage);
            Assert.AreEqual("some info", car.Info);
            Assert.AreEqual(5000, car.Price);
        }
        [Test]
        public async Task carController_Add_AddCarToDB()
        {
            var car = new CarExchangeAPI.Orchestrator.Cars.Contracts.Car() { Brand = 1, Damage = false, Info = "some info", Price = 10000 };
            var content = new StringContent(JsonConvert.SerializeObject(car), Encoding.UTF8, "application/json");
            var httpresponse = await _client.PostAsync(RequestUrl + 1, content);

            httpresponse.EnsureSuccessStatusCode();
            var stringresponse = await httpresponse.Content.ReadAsStringAsync();
            var carInResponse = JsonConvert.DeserializeObject<Orchestrator.Cars.Contracts.Car>(stringresponse);

            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<CarDbContext>();
                var databaseCar = await context.Cars.FindAsync(carInResponse.Id);
                Assert.AreEqual(databaseCar.Id, carInResponse.Id);
                Assert.AreEqual(databaseCar.Brand, carInResponse.Brand);
                Assert.AreEqual(databaseCar.Damage, carInResponse.Damage);
                Assert.AreEqual(databaseCar.Info, carInResponse.Info);
                Assert.AreEqual(databaseCar.Price, carInResponse.Price);
            }

        }
        [Test]
        public async Task carController_Update_UpdateCarInDatabase()
        {
            var car = new CarExchangeAPI.Orchestrator.Cars.Contracts.Car() {Id = 1, Price = 10000};
            var content = new StringContent(JsonConvert.SerializeObject(car), Encoding.UTF8, "application/json");
            var httpresponse = await _client.PatchAsync($"/api/Cars/{car.Id}", content);

            httpresponse.EnsureSuccessStatusCode();
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<CarDbContext>();
                var databaseCar = await context.Cars.FindAsync(car.Id);
                Assert.AreEqual(car.Id, databaseCar.Id);
            }
        }
        [Test]
        public async Task CarController_DeleteById_DeletesCarFromDatabase()
        {
            var carId = 1;
            var httpResponse = await _client.DeleteAsync("api/Cars/" + carId);

            httpResponse.EnsureSuccessStatusCode();

            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<CarDbContext>();
                Assert.AreEqual(0, context.Cars.Count());
            }
        }
    }
}

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
    [TestFixture]
    public class UserIntegrationTests
    {
        private HttpClient _client;
        private CustomWebApplicationFactory _factory;
        private const string RequestUrl = "api/User/";

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }
        [Test]
        public async Task usersController_GetById_ReturnModel()
        {
            var httpResponse = await _client.GetAsync(RequestUrl + 1);

            httpResponse.EnsureSuccessStatusCode();
            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<CarExchangeAPI.Orchestrator.Users.Contracts.User>(stringResponse);

            Assert.AreEqual(1, user.Id);
            Assert.AreEqual("Jora", user.Name);
            Assert.AreEqual("Oliv", user.Surname);
        }
        [Test]
        public async Task UserController_Add_AddsUserToDatabase()
        {
            var user = new Orchestrator.Users.Contracts.User() { Name = "Alex", Surname = "Breaman" };      
            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var httpresponse = await _client.PostAsync(RequestUrl + 1, content);

            httpresponse.EnsureSuccessStatusCode();
            var stringResponse = await httpresponse.Content.ReadAsStringAsync();
            var userInResponse = JsonConvert.DeserializeObject<Orchestrator.Users.Contracts.User>(stringResponse);           
        
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<CarDbContext>();
                var databaseCarEx = await context.Users.FindAsync(userInResponse.Id);

                Assert.AreEqual(databaseCarEx.Id, userInResponse.Id);
                Assert.AreEqual(databaseCarEx.Name, userInResponse.Name);
                Assert.AreEqual(databaseCarEx.Surname, userInResponse.Surname);
            }
        }
        [Test]
        public async Task UserController_DeleteById_DeleteUserFromDB()
        {
            var userId = 1;
            var httpresponse = await _client.DeleteAsync(RequestUrl + userId);

            httpresponse.EnsureSuccessStatusCode();
            using (var test = _factory.Services.CreateScope())
            {
                var context = test.ServiceProvider.GetService<CarDbContext>();
                Assert.AreEqual(0, context.Users.Count());

            }
        }
    }
}

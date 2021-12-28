using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CarExchangeAPI.Dao;
using Microsoft.EntityFrameworkCore.InMemory;
using AutoMapper;
using CarExchangeAPI.Dao.Car;

namespace CarExchangeAPI.Tests
{
    public class UnitTestHelper
    {

        public static DbContextOptions<CarDbContext> GetDbContextOptions()
        {
            var options = new DbContextOptionsBuilder<CarDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using (var context = new CarDbContext(options))
            {
                SeedData(context);
            }
            return options;
        }

        public static void SeedData(CarDbContext context)
        {
            context.Users.Add(new Dao.User.UserDto { Id = 1, Name = "Jora", Surname = "Oliv" });
            context.Cars.Add(new Dao.Car.CarDto { Id = 1, Brand = 1, Damage = false, Info = "some info", Price = 5000 });
            context.SaveChanges();
        }

        public static Mapper CreateMapperProfile()
        {
            var myProfile = new CarDaoProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));

            return new Mapper(configuration);
        }

    }
}

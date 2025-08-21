using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using InforceTestTask.Services.Implementations;
using InforceTestTask.Models;
using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;

namespace TestTaskTests
{
    public class AuthServiceTests
    {
        private TestTaskAppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<TestTaskAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new TestTaskAppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private IConfiguration GetConfig()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AppSettings:AccessToken", "supersecretkeysupersecretkeysupersecretkeysupersecretkeysupersecretkeysupersecretkeysupersecretkeysupersecretkey"},
                {"AppSettings:Issuer", "TestIssuer"},
                {"AppSettings:Audience", "TestAudience"}
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsAuthResponse_WhenDataIsValid()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new RegisterRequest
            {
                UserName = "testuser",
                Email = "test@email.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Role = "User"
            };

            var result = await service.RegisterAsync(request);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task RegisterAsync_ReturnsNull_WhenUserAlreadyExists()
        {
            var dbContext = GetDbContext();
            dbContext.Users.Add(new Users { user_name = "testuser", email = "test@email.com", hashed_password = "hash", role = "User" });
            dbContext.SaveChanges();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new RegisterRequest
            {
                UserName = "testuser",
                Email = "test@email.com",
                Password = "Test123!",
                ConfirmPassword = "Test123!",
                Role = "User"
            };

            var result = await service.RegisterAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ReturnsNull_WhenPasswordsDoNotMatch()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new RegisterRequest
            {
                UserName = "testuser2",
                Email = "test2@email.com",
                Password = "Test123!",
                ConfirmPassword = "Test1234!",
                Role = "User"
            };

            var result = await service.RegisterAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new LoginRequest
            {
                UserName = "nouser",
                Password = "Test123!"
            };

            var result = await service.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenPasswordIsIncorrect()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "User",
                hashed_password = new Microsoft.AspNetCore.Identity.PasswordHasher<Users>().HashPassword(null, "CorrectPassword")
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new LoginRequest
            {
                UserName = "testuser",
                Password = "WrongPassword"
            };

            var result = await service.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ReturnsAuthResponse_WhenCredentialsAreValid()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "User",
                hashed_password = new Microsoft.AspNetCore.Identity.PasswordHasher<Users>().HashPassword(null, "Test123!")
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new LoginRequest
            {
                UserName = "testuser",
                Password = "Test123!"
            };

            var result = await service.LoginAsync(request);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new RefreshTokenRequest
            {
                Id = 999,
                AccessToken = "fake"
            };

            var result = await service.RefreshTokenAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenRefreshTokenIsInvalid()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "User",
                hashed_password = "hash",
                refresh_token = "validtoken",
                refresh_token_expiry_time = DateTime.UtcNow.AddDays(1)
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new RefreshTokenRequest
            {
                Id = user.id,
                AccessToken = "invalidtoken"
            };

            var result = await service.RefreshTokenAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsNull_WhenRefreshTokenExpired()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "User",
                hashed_password = "hash",
                refresh_token = "validtoken",
                refresh_token_expiry_time = DateTime.UtcNow.AddDays(-1)
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new RefreshTokenRequest
            {
                Id = user.id,
                AccessToken = "validtoken"
            };

            var result = await service.RefreshTokenAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task RefreshTokenAsync_ReturnsAuthResponse_WhenRefreshTokenIsValid()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "User",
                hashed_password = "hash",
                refresh_token = "validtoken",
                refresh_token_expiry_time = DateTime.UtcNow.AddDays(1)
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new RefreshTokenRequest
            {
                Id = user.id,
                AccessToken = "validtoken"
            };

            var result = await service.RefreshTokenAsync(request);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.AccessToken));
            Assert.False(string.IsNullOrEmpty(result.RefreshToken));
        }

        [Fact]
        public async Task GetUserIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var request = new UserIdRoleRequest
            {
                UserName = "nouser"
            };

            var result = await service.GetUserIdAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserIdAsync_ReturnsUserIdRoleResponse_WhenUserExists()
        {
            var dbContext = GetDbContext();
            var config = GetConfig();
            var service = new AuthService(dbContext, config);

            var user = new Users
            {
                user_name = "testuser",
                email = "test@email.com",
                role = "Admin",
                hashed_password = "hash"
            };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var request = new UserIdRoleRequest
            {
                UserName = "testuser"
            };

            var result = await service.GetUserIdAsync(request);

            Assert.NotNull(result);
            Assert.Equal(user.id, result.UserId);
            Assert.Equal(user.role, result.Role);
        }
    }
}
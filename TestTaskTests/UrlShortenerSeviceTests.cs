using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using InforceTestTask.Services.Implementations;
using InforceTestTask.Models;
using InforceTestTask.DTOs.Requests;
using InforceTestTask.DTOs.Responses;
using Microsoft.EntityFrameworkCore.InMemory;

namespace TestTaskTests
{
    public class UrlShortenerSeviceTests
    {
        private TestTaskAppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<TestTaskAppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestTaskAppDbContext(options);
        }

        [Fact]
        public async Task CreateShortUrlAsync_ThrowsException_WhenUrlIsInvalid()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlTableRequest { UserId = 1, LongUrl = "invalid-url" };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateShortUrlAsync(request));
        }

        [Fact]
        public async Task CreateShortUrlAsync_ThrowsException_WhenUserNotFound()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlTableRequest { UserId = 999, LongUrl = "https://google.com" };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateShortUrlAsync(request));
        }

        [Fact]
        public async Task CreateShortUrlAsync_ReturnsResponse_WhenDataIsValid()
        {
            var dbContext = GetDbContext();
            dbContext.Users.Add(new Users { id = 1, user_name = "testuser" });
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlTableRequest { UserId = 1, LongUrl = "https://google.com" };

            var result = await service.CreateShortUrlAsync(request);

            Assert.NotNull(result);
            Assert.Equal("https://google.com", result.LongUrl);
            Assert.StartsWith("https://localhost:7033/", result.ShortUrl);
        }

        [Fact]
        public async Task GetShortUrlInfoAsync_ReturnsNull_WhenNotFound()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlInfoRequest { UrlId = 1 };

            var result = await service.GetShortUrlInfoAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetShortUrlInfoAsync_ReturnsInfo_WhenExists()
        {
            var dbContext = GetDbContext();
            var user = new Users { id = 1, user_name = "testuser" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "123",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            dbContext.Users.Add(user);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlInfoRequest { UrlId = 1 };

            var result = await service.GetShortUrlInfoAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.UrlId);
            Assert.Equal("https://google.com", result.LongUrl);
            Assert.Equal("testuser", result.CreatedBy);
        }

        [Fact]
        public async Task OpenShortUrlAsync_ThrowsException_WhenCodeIsNullOrEmpty()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlOpenRequest { Code = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => service.OpenShortUrlAsync(request));
        }

        [Fact]
        public async Task OpenShortUrlAsync_ReturnsNull_WhenCodeNotFound()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlOpenRequest { Code = "notfound" };

            var result = await service.OpenShortUrlAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task OpenShortUrlAsync_ReturnsLongUrl_WhenCodeExists()
        {
            var dbContext = GetDbContext();
            var user = new Users { id = 1, user_name = "testuser" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            dbContext.Users.Add(user);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlOpenRequest { Code = "abc" };

            var result = await service.OpenShortUrlAsync(request);

            Assert.Equal("https://google.com", result);
        }

        [Fact]
        public async Task DeleteShortUrlAsync_ThrowsException_WhenUrlNotFound()
        {
            var dbContext = GetDbContext();
            dbContext.Users.Add(new Users { id = 1, user_name = "testuser", role = "User" });
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlDeleteRequest { UrlId = 999, UserId = 1 };

            await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteShortUrlAsync(request));
        }

        [Fact]
        public async Task DeleteShortUrlAsync_ThrowsException_WhenUserNotFound()
        {
            var dbContext = GetDbContext();
            var user = new Users { id = 1, user_name = "testuser", role = "User" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            dbContext.Users.Add(user);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlDeleteRequest { UrlId = 1, UserId = 999 };

            await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteShortUrlAsync(request));
        }

        [Fact]
        public async Task DeleteShortUrlAsync_ThrowsException_WhenNoPermission()
        {
            var dbContext = GetDbContext();
            var user1 = new Users { id = 1, user_name = "testuser1", role = "User" };
            var user2 = new Users { id = 2, user_name = "testuser2", role = "User" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "testuser1",
                created_date = DateTime.Now,
                user = user1
            };
            dbContext.Users.Add(user1);
            dbContext.Users.Add(user2);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlDeleteRequest { UrlId = 1, UserId = 2 };

            await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteShortUrlAsync(request));
        }

        [Fact]
        public async Task DeleteShortUrlAsync_ReturnsTrue_WhenUserIsOwner()
        {
            var dbContext = GetDbContext();
            var user = new Users { id = 1, user_name = "testuser", role = "User" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            dbContext.Users.Add(user);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlDeleteRequest { UrlId = 1, UserId = 1 };

            var result = await service.DeleteShortUrlAsync(request);

            Assert.True(result);
            Assert.Empty(dbContext.ShortURLs);
        }

        [Fact]
        public async Task DeleteShortUrlAsync_ReturnsTrue_WhenUserIsAdmin()
        {
            var dbContext = GetDbContext();
            var owner = new Users { id = 1, user_name = "owner", role = "User" };
            var admin = new Users { id = 2, user_name = "admin", role = "Admin" };
            var url = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "owner",
                created_date = DateTime.Now,
                user = owner
            };
            dbContext.Users.Add(owner);
            dbContext.Users.Add(admin);
            dbContext.ShortURLs.Add(url);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var request = new ShortUrlDeleteRequest { UrlId = 1, UserId = 2 };

            var result = await service.DeleteShortUrlAsync(request);

            Assert.True(result);
            Assert.Empty(dbContext.ShortURLs);
        }

        [Fact]
        public async Task GetAllShortUrlsAsync_ReturnsEmptyList_WhenNoUrls()
        {
            var dbContext = GetDbContext();
            var service = new UrlShortenerService(dbContext);

            var result = await service.GetAllShortUrlsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllShortUrlsAsync_ReturnsList_WhenUrlsExist()
        {
            var dbContext = GetDbContext();
            var user = new Users { id = 1, user_name = "testuser" };
            var url1 = new ShortURLs
            {
                id = 1,
                long_url = "https://google.com",
                short_url = "https://localhost:7033/abc",
                code = "abc",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            var url2 = new ShortURLs
            {
                id = 2,
                long_url = "https://microsoft.com",
                short_url = "https://localhost:7033/xyz",
                code = "xyz",
                created_by = "testuser",
                created_date = DateTime.Now,
                user = user
            };
            dbContext.Users.Add(user);
            dbContext.ShortURLs.Add(url1);
            dbContext.ShortURLs.Add(url2);
            await dbContext.SaveChangesAsync();

            var service = new UrlShortenerService(dbContext);

            var result = await service.GetAllShortUrlsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.LongUrl == "https://google.com");
            Assert.Contains(result, r => r.LongUrl == "https://microsoft.com");
        }
    }
}
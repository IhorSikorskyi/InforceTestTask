using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;


namespace InforceTestTask.Models;

public class TestTaskAppDbContext(DbContextOptions<TestTaskAppDbContext> options) : DbContext(options)
{
    public DbSet<Users> Users { get; set; }
    public DbSet<ShortURLs> ShortURLs { get; set; }


}
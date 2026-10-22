using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Usuarios { get; set; }
    public DbSet<Campaign> Campanhas { get; set; }
    public DbSet<Donation> Doacoes { get; set; }
}
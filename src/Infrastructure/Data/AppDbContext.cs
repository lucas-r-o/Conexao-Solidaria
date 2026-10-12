using Microsoft.EntityFrameworkCore;
using Models;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Campanha> Campanhas { get; set; }
    public DbSet<Doacao> Doacoes { get; set; }
}
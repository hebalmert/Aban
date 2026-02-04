using Aban.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Aban.AppInfra;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    //Entities

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Manager> Managers => Set<Manager>();
    public DbSet<Corporation> Corporations => Set<Corporation>();
    public DbSet<UserRoleDetails> UserRoleDetails => Set<UserRoleDetails>();



    //Esta parte nos permite tomar las configuraciones desde otra ubicacion, para mantener el codigo mas ordenado
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Para tomar los calores de ConfigEntities
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

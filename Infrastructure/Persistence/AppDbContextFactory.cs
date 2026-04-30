using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OficinaMecanica.Api.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = "server=localhost;port=3306;database=oficina_mecanica;user=oficina_user;password=oficina_pass";

        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0)));

        return new AppDbContext(optionsBuilder.Options);
    }
}

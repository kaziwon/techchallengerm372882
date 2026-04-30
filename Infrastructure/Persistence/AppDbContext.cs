using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Domain.Entities;

namespace OficinaMecanica.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Veiculo> Veiculos { get; set; }
    public DbSet<Servico> Servicos { get; set; }
    public DbSet<PecaInsumo> PecasInsumos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasIndex(cliente => cliente.CpfCnpj)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasMany(cliente => cliente.Veiculos)
            .WithOne(veiculo => veiculo.Cliente)
            .HasForeignKey(veiculo => veiculo.ClienteId);

        modelBuilder.Entity<Veiculo>()
            .HasIndex(veiculo => veiculo.Placa)
            .IsUnique();

        modelBuilder.Entity<Servico>()
            .HasIndex(servico => servico.Nome)
            .IsUnique();

        modelBuilder.Entity<PecaInsumo>()
            .HasIndex(pecaInsumo => pecaInsumo.Nome)
            .IsUnique();
    }
}

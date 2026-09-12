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
    public DbSet<OrdemServico> OrdensServico { get; set; }
    public DbSet<OrdemServicoItemServico> OrdemServicoItensServico { get; set; }
    public DbSet<OrdemServicoItemPecaInsumo> OrdemServicoItensPecaInsumo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasIndex(cliente => cliente.CpfCnpj)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .Property(cliente => cliente.Ativo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Cliente>()
            .HasMany(cliente => cliente.Veiculos)
            .WithOne(veiculo => veiculo.Cliente)
            .HasForeignKey(veiculo => veiculo.ClienteId);

        modelBuilder.Entity<Cliente>()
            .HasMany(cliente => cliente.OrdensServico)
            .WithOne(ordemServico => ordemServico.Cliente)
            .HasForeignKey(ordemServico => ordemServico.ClienteId);

        modelBuilder.Entity<Veiculo>()
            .HasMany(veiculo => veiculo.OrdensServico)
            .WithOne(ordemServico => ordemServico.Veiculo)
            .HasForeignKey(ordemServico => ordemServico.VeiculoId);

        modelBuilder.Entity<Veiculo>()
            .HasIndex(veiculo => veiculo.Placa)
            .IsUnique();

        modelBuilder.Entity<Servico>()
            .HasIndex(servico => servico.Nome)
            .IsUnique();

        modelBuilder.Entity<PecaInsumo>()
            .HasIndex(pecaInsumo => pecaInsumo.Nome)
            .IsUnique();

        modelBuilder.Entity<OrdemServicoItemServico>()
            .HasOne(item => item.OrdemServico)
            .WithMany(ordemServico => ordemServico.ItensServico)
            .HasForeignKey(item => item.OrdemServicoId);

        modelBuilder.Entity<OrdemServicoItemPecaInsumo>()
            .HasOne(item => item.OrdemServico)
            .WithMany(ordemServico => ordemServico.ItensPecaInsumo)
            .HasForeignKey(item => item.OrdemServicoId);

        modelBuilder.Entity<OrdemServico>()
            .Property(ordemServico => ordemServico.EnvioOrcamento)
            .HasColumnName("MockEnvioOrcamento");
    }
}

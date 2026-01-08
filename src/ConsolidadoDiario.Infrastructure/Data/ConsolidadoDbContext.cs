using ConsolidadoDiario.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsolidadoDiario.Infrastructure.Data;

public class ConsolidadoDbContext : DbContext
{
    public ConsolidadoDbContext(DbContextOptions<ConsolidadoDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.ConsolidadoDiario> Consolidados { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.ConsolidadoDiario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UsuarioId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.TotalCreditos).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.TotalDebitos).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.SaldoDiario).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.QuantidadeLancamentos).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired();

            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => new { e.UsuarioId, e.Data }).IsUnique();
        });
    }
}


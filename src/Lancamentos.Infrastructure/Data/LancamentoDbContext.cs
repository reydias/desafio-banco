using Lancamentos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lancamentos.Infrastructure.Data;

public class LancamentoDbContext : DbContext
{
    public LancamentoDbContext(DbContextOptions<LancamentoDbContext> options) : base(options)
    {
    }

    public DbSet<Lancamento> Lancamentos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Lancamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.UsuarioId).IsRequired();
            entity.Property(e => e.Data).IsRequired();
            entity.Property(e => e.Valor).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasMaxLength(1)
                .HasConversion<string>(
                    v => v == TipoLancamento.Credito ? "C" : "D",
                    v => v == "C" ? TipoLancamento.Credito : TipoLancamento.Debito);
            entity.Property(e => e.Descricao).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();

            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Data);
            entity.HasIndex(e => new { e.UsuarioId, e.Data });
            entity.HasIndex(e => new { e.Data, e.Tipo });
        });
    }
}


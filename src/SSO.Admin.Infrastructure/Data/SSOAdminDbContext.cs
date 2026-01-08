using SSO.Admin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SSO.Admin.Infrastructure.Data;

public class SSOAdminDbContext : DbContext
{
    public SSOAdminDbContext(DbContextOptions<SSOAdminDbContext> options) : base(options)
    {
    }

    public DbSet<UsuarioToken> UsuariosToken { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UsuarioToken>(entity =>
        {
            entity.ToTable("UsuariosToken");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Login).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SenhaHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Ativo).IsRequired();
            entity.Property(e => e.DataCriacao).IsRequired();
            entity.Property(e => e.DataAtualizacao).IsRequired(false);

            entity.HasIndex(e => e.Login).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}




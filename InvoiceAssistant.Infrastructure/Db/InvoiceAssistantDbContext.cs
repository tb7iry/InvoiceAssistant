using InvoiceAssistant.Domain.Entites;
using Microsoft.EntityFrameworkCore;

namespace InvoiceAssistant.Infrastructure.Db;

public class InvoiceAssistantDbContext : DbContext
{
    public InvoiceAssistantDbContext(DbContextOptions<InvoiceAssistantDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ClientName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TotalAmount)
                .HasPrecision(18, 2);

            entity.HasIndex(e => e.InvoiceNumber)
                .IsUnique();

            entity.HasMany(e => e.InvoiceDetails)
                .WithOne(e => e.Invoice)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.ToTable("InvoiceDetails");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ItemName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }

}

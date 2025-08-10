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
        var now = DateTimeOffset.UtcNow;

        // Seed Invoices
        var invoices = new List<Invoice>
    {
        new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-1001",
            ClientName = "Acme Corp",
            IssueDate = now.AddDays(-10),
            DueDate = now.AddDays(20),
            Status = "Paid",
            Currency = "EGP",
            TotalAmount = 1500.00m,
            PaidAmount = 1500.00m,
            Salesperson = "Ahmed",
            Branch = "Cairo"
        },
        new Invoice
        {
            Id = 2,
            InvoiceNumber = "INV-1002",
            ClientName = "Beta LLC",
            IssueDate = now.AddDays(-40),
            DueDate = now.AddDays(-10),
            Status = "Overdue",
            Currency = "EGP",
            TotalAmount = 2500.00m,
            PaidAmount = 500.00m,
            Salesperson = "Sara",
            Branch = "Alexandria"
        },
        new Invoice
        {
            Id = 3,
            InvoiceNumber = "INV-1003",
            ClientName = "Gamma Ltd",
            IssueDate = now.AddDays(-5),
            DueDate = now.AddDays(25),
            Status = "Unpaid",
            Currency = "EGP",
            TotalAmount = 1200.00m,
            PaidAmount = 0.00m,
            Salesperson = "Omar",
            Branch = "Cairo"
        },
        new Invoice
        {
            Id = 4,
            InvoiceNumber = "INV-1004",
            ClientName = "Acme Corp",
            IssueDate = now.AddMonths(-2).AddDays(-3),
            DueDate = now.AddMonths(-2).AddDays(27),
            Status = "Paid",
            Currency = "EGP",
            TotalAmount = 3000.00m,
            PaidAmount = 3000.00m,
            Salesperson = "Ahmed",
            Branch = "Alexandria"
        },
        new Invoice
        {
            Id = 5,
            InvoiceNumber = "INV-1005",
            ClientName = "Delta Inc",
            IssueDate = now.AddMonths(-1).AddDays(-5),
            DueDate = now.AddMonths(-1).AddDays(25),
            Status = "Unpaid",
            Currency = "EGP",
            TotalAmount = 500.00m,
            PaidAmount = 0.00m,
            Salesperson = "Sara",
            Branch = "Cairo"
        }
    };
        modelBuilder.Entity<Invoice>().HasData(invoices);

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

        var invoiceDetails = new List<InvoiceDetail>
    {
        new InvoiceDetail
        {
            Id = 1,
            InvoiceId = 1,
            ItemSku = "P001",
            ItemName = "Printer",
            Quantity = 1,
            UnitPrice = 1500.00m
        },
        new InvoiceDetail
        {
            Id = 2,
            InvoiceId = 2,
            ItemSku = "L002",
            ItemName = "Laptop",
            Quantity = 1,
            UnitPrice = 2500.00m
        },
        new InvoiceDetail
        {
            Id = 3,
            InvoiceId = 3,
            ItemSku = "K003",
            ItemName = "Keyboard",
            Quantity = 2,
            UnitPrice = 600.00m
        },
        new InvoiceDetail
        {
            Id = 4,
            InvoiceId = 4,
            ItemSku = "M004",
            ItemName = "Monitor",
            Quantity = 2,
            UnitPrice = 1500.00m
        },
        new InvoiceDetail
        {
            Id = 5,
            InvoiceId = 5,
            ItemSku = "M005",
            ItemName = "Mouse",
            Quantity = 5,
            UnitPrice = 100.00m
        }
    };
    
        modelBuilder.Entity<InvoiceDetail>().HasData(invoiceDetails);

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

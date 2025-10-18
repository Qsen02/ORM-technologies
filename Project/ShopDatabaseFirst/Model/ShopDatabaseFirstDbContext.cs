using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShopDatabaseFirst.Model;

public partial class ShopDatabaseFirstDbContext : DbContext
{
    public ShopDatabaseFirstDbContext()
    {
    }

    public ShopDatabaseFirstDbContext(DbContextOptions<ShopDatabaseFirstDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb; Database=ShopDatabaseFirstDB;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_Sales_ProductId");

            entity.HasOne(d => d.Product).WithMany(p => p.Sales).HasForeignKey(d => d.ProductId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

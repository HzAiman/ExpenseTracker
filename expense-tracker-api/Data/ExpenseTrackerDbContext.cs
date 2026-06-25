using ExpenseTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTrackerApi.Data;

public sealed class ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).HasMaxLength(80).IsRequired();
            entity.HasIndex(category => category.Name).IsUnique();

            entity.HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Transport" },
                new Category { Id = 3, Name = "Housing" },
                new Category { Id = 4, Name = "Utilities" },
                new Category { Id = 5, Name = "Healthcare" },
                new Category { Id = 6, Name = "Entertainment" },
                new Category { Id = 7, Name = "Education" },
                new Category { Id = 8, Name = "Other" });
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(expense => expense.Id);
            entity.Property(expense => expense.Amount).HasColumnType("decimal(18,2)");
            entity.Property(expense => expense.Description).HasMaxLength(240).IsRequired();
            entity.Property(expense => expense.Date).IsRequired();

            entity.HasOne(expense => expense.Category)
                .WithMany(category => category.Expenses)
                .HasForeignKey(expense => expense.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

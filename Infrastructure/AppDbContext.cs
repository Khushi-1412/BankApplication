using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;


namespace BankApplication.Infrastructure
{
    public class AppDbContext: DbContext , IUnitOfWork
    {
        public AppDbContext(DbContextOptions con): base(con) 
        {
        
        }
        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {

            await base.SaveChangesAsync(cancellationToken);

            return true;
        }
        public DbSet<BankAccount> Banks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BankAccount>(x =>
            {
                x.ToTable("Bank");
                x.HasKey(x => x.Id);
                x.Property(x => x.Id);
                x.Property(x => x.AccountHolderName).IsRequired();
                x.Property(x => x.AccountNumber).IsRequired();
                x.Property(x => x.Email).IsRequired();
                x.Property(x => x.Phone).IsRequired();
                x.Property(x => x.Gender).IsRequired();
                x.Property(x => x.Age).IsRequired();
                x.Property(x => x.Address).IsRequired();
                x.Property(x => x.AccountBalance).IsRequired();

            });
        }
    }
    
}

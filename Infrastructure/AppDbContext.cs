using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                x.Property(x => x.Name).IsRequired();
                x.Property(x => x.Balance).IsRequired();
                x.Property(x => x.Email).IsRequired();
                x.Property(x => x.Address).IsRequired();
                x.Property(x => x.AccountNumber).IsRequired();
                x.Property(x => x.Balance).IsRequired();

            });
        }
    }
    
}

using BankApplication.Domain.Base;
using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Infrastructure
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _appDbContext;

        public IUnitOfWork UnitOfWork => _appDbContext;

        public AccountRepository(AppDbContext context)
        {
            _appDbContext = context ?? throw new ArgumentNullException(nameof(context));
        }
        

        public async Task<BankAccount?> GetById(Guid id)
        {
                return await _appDbContext.Banks.Where(b => b.Id == id).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<BankAccount>> List()
        {
            return await _appDbContext.Banks
                .ToListAsync(); ;
        }

        public async Task<BankAccount> Add(BankAccount bankAccount)
        {
            var account = _appDbContext.Banks.Add(bankAccount).Entity;
            await _appDbContext.SaveChangesAsync();
            return account;

        }

        public async Task Update(BankAccount bankAccount)
        {
            var account = _appDbContext.Banks.Update(bankAccount).Entity;
            await _appDbContext.SaveChangesAsync();
            
        }
        
       
        public async Task Delete(BankAccount bankAccount)
        {
            _appDbContext.Banks.Remove(bankAccount);
            await _appDbContext.SaveChangesAsync();
            
        }
    }
}
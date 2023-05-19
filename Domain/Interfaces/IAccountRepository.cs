using BankApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Domain.Interfaces
{
    public interface IAccountRepository : IRepository<BankAccount>
    {
        Task<BankAccount?> GetById(Guid id);
        Task<IEnumerable<BankAccount>> List();
        Task<BankAccount> Add(BankAccount bankAccount);
        Task Update(BankAccount bankAccount);
        Task Delete(BankAccount bankAccount);
 

      
    }
}

using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Domain.Services
{
    public class BankService
    {
        private readonly IAccountRepository _accountRepository;
        public BankService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        public BankAccount CreateBank(string name, string email, string phone, string address, long accountNumber, int balance = 0)
        {
           
            var bank = new BankAccount(name, email, phone, address, accountNumber, balance);
            _accountRepository.Add(bank);

            return bank;
        }

        //public void Deposit(Guid accountId, int amount)
        //{
        //    var bank = _accountRepository.GetById(accountId);
        //    bank.Balance += amount;
        //    _accountRepository.Update(bank);
        //}

        //public void Withdraw(Guid accountId, int amount)
        //{
        //    var bank = _accountRepository.GetById(accountId);
        //    if (bank.Balance < amount)
        //    {
        //        throw new InvalidOperationException("Insufficient balance");
        //    }
        //    bank.Balance -= amount;
        //    _accountRepository.Update(bank);
        //}
    }
}

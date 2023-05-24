using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BankApplication.Domain.Services
{
    public class BankService
    {
        private readonly IAccountRepository _accountRepository;
        public BankService(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }


        public async void Transfer(Guid sourceAccountId, Guid destinationAccountId, decimal amount)
        {
            var SourceAccount = await _accountRepository.GetById(sourceAccountId);
            var DestinationAccount = await _accountRepository.GetById(destinationAccountId);

            if (SourceAccount is null)
                throw new InvalidOperationException("The Source Account Number is Invalid.");

            if (DestinationAccount is null)
                throw new InvalidOperationException("The Destination Account Number is Invalid.");

            using var txnScope = new TransactionScope(TransactionScopeOption.RequiresNew);
            try
            {
                SourceAccount.Withdraw(amount);
                DestinationAccount.Deposit(amount);

                txnScope.Complete();
            }
            catch (Exception ex)
            {
                txnScope.Dispose();

                throw new Exception(ex.Message);
                throw new Exception("Transaction Failed");
            }
           


        }
    }
}

using BankApplication.Domain.Base;
using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BankApplication.Domain.Models
{
    public class BankAccount : Entity
    {
        public BankAccount() { }
    

        public BankAccount(string accountHolderName, string email, string phone, string gender, int age,string address, decimal accountBalance)
        {
            AccountHolderName = accountHolderName;

            Email = email;
            Phone = phone;
            Gender = gender;
            Age= age;
            Address = address;
            AccountBalance = accountBalance;
        }

        public string AccountHolderName { get; private set; }
        public string AccountNumber { get; private set; } = Generator.GetRandomNumbers(10);       
        public string Email { get; private set; }
        public string Phone { get; private set; }
        public string Gender { get; private set; }
        public int Age { get; private set; }   
        public string Address { get; private set; }
        public decimal AccountBalance { get; private set; }
      

        private void UpdateBalance(decimal updatedBalance)
        {
            if (updatedBalance < 0m)
                throw new InvalidOperationException("$ Enter valid amount");

            AccountBalance = updatedBalance;
        }
        public void Deposit(decimal amount)
        {
            if (amount <= 0m)
            {
                throw new InvalidOperationException("Amount entered is less than 0");
            }
            
            UpdateBalance(AccountBalance + amount);
        }
        public void Withdraw(decimal amount)
        {
            
            if (AccountBalance < amount)
            {
                throw new InvalidOperationException($"Not enough money to withdraw. Balance :{AccountBalance}");
            }
            if (amount <= 0)
            {
                throw new InvalidOperationException("Amount entered is less than 0");
            }
            UpdateBalance(AccountBalance - amount);
        }
       



    }

}

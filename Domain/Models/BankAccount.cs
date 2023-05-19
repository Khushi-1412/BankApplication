using BankApplication.Domain.Base;
using BankApplication.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApplication.Domain.Models
{
    public class BankAccount : Entity
    {
        public BankAccount() { }
        public BankAccount(string name, string email, string phone, string address, long accountNumber, int balance)
        {
            
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
            AccountNumber = accountNumber;
            Balance = balance;
        }
        public string Name { get; private set; } 
        public string Email { get; private set; } 
        public string Phone { get; private set; }
        public string Address { get; private set; }
        public long AccountNumber { get; private set; }
        public int Balance { get; private set; }
     
        private void UpdateBalance(int updatedBalance)
        {
            if (updatedBalance < 0m)
                throw new InvalidOperationException("$ Enter valid amount");

            Balance = updatedBalance;
        }
        public void Deposit(int amount)
        {
            if (amount <= 0m)
            {
                throw new InvalidOperationException("$ Amount entered is less than 0");
            }
            
                UpdateBalance(Balance+amount);
          }
        public void Withdraw(int amount)
        {

            if (amount <= 0)
            {
                throw new InvalidOperationException("Not valid");
            }
            UpdateBalance(Balance - amount);
        }



        }

}

using BankApplication.Domain.Models;

namespace BankApplication.UnitTests
{
    public class BankAccountUnitTest
    {
       

        [Fact]
        public void DepositBalanceCorrectly()
        {
            var account = new BankAccount();
            var amount = 1000;
            account.Deposit(1000);
            Assert.Equal(amount, account.AccountBalance);
        }

        [Fact]
        public void  ThrowsExceptionIfDepositAmountIsZero()
        {
            var account = new BankAccount();
            var amount = 0;

            Assert.Throws<InvalidOperationException>(() => account.Deposit(amount));
        }

        [Fact]

        public void ThrowsExceptionIfDepositAmountIsNegative()
        {
            var account = new BankAccount();
            var amount = -100;
            Assert.Throws<InvalidOperationException>(() => account.Deposit(amount));
        }

        [Fact]
        public void WithdrawsAmountCorrectly()
        {
            var account = new BankAccount("Pratiti","pra@gmail.com","876543280","Female",20,"Pune",500);
            var amount = 10;
            var remaining_amount = account.AccountBalance - amount;
            account.Withdraw(amount);
            Assert.Equal(remaining_amount, account.AccountBalance);
            
        }

        [Fact]
        public void ThrowsExceptionWhenWithdrawBalanceIsMoreThanAccountBalance() {
            var account = new BankAccount("Pratiti", "pra@gmail.com", "876543280", "Female", 20, "Pune", 500);
            var withDrawAmount = 1000; 

            Assert.Throws<InvalidOperationException>(() => account. Withdraw(withDrawAmount));

        }

        [Fact]
        public void ThrowsExceptionIfWithdrawAmountIsZero()
        {
            var account = new BankAccount();
            var amount = 0;

            Assert.Throws<InvalidOperationException>(() => account.Withdraw(amount));
        }


    }
}
using BankApplication.Domain.Models;

namespace BankApplication.UnitTests
{
    public class BankAccountUnitTest
    {

        [Fact]
        public void UpdatesBalanceCorrectly()
        {
            var account = new BankAccount();
            var amount = 1000;
            account.Deposit(1000);
            Assert.Equal(amount, account.Balance);

        }

        [Fact]
        public void throwsExceptionIfAmountIsZero()
        {
            var account = new BankAccount();
            var amount = 0;

            Assert.Throws<InvalidOperationException>(() => account.Deposit(amount));
        }
    }
}
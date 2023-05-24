using BankApplication.Api.DTO;
using BankApplication.Domain.Interfaces;
using BankApplication.Domain.Models;
using BankApplication.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankApplication.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;

        public BankController(IAccountRepository accountRepository)
        {
            this._accountRepository = accountRepository;

        }
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<BankAccount>>> Index()
        {
            var accounts = await _accountRepository.List();
            return Ok(accounts);
        }


        [HttpGet("accountNumber")]
        public async Task<ActionResult<BankAccount?>> GetUserAccount(string accountNumber)
        {
            var bank = await _accountRepository.GetByAccount(accountNumber);
            if (bank == null) return NotFound();
            return bank;
        }
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Post(CreateBankDTO dto)
        {
            BankAccount bank = new BankAccount(dto.AccountHolderName, dto.Email, dto.Phone, dto.Gender, dto.Age, dto.Address, dto.AccountBalance);

            await _accountRepository.Add(bank);
            return CreatedAtAction
                (
                nameof(GetUserAccount),
                new { id = bank.Id },
                bank
                );
        }
        [HttpPut("{accno}/deposit")]
        public async Task<IActionResult> DepositMoney(string accno, decimal amount)
        {
            var account = await _accountRepository.GetByAccount(accno);
            if (account is null)
                return NotFound();

            account.Deposit(amount);

            await _accountRepository.Update(account);
            return AcceptedAtAction
                (
                nameof(GetUserAccount),
                new { id = account.Id },
                account
                );
        }
        [HttpPut("{accno}/withdraw")]
        public async Task<IActionResult> WithdrawMoney(string accno, decimal amount)
        {
            var account = await _accountRepository.GetByAccount(accno);
            if (account is null)
                return NotFound();

            account.Withdraw(amount);

            await _accountRepository.Update(account);
            return AcceptedAtAction(nameof(GetUserAccount),
                new { id = account.Id },
                account);
        }

        [HttpDelete("{id}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bank = await _accountRepository.GetById(id);
            if (bank is null) return NotFound();

            await _accountRepository.Delete(bank);

            return NoContent();



        }
        [HttpPut("transfer")]
        public async Task<IActionResult> Transfer(string sourceAccountNumber, string destinationAccountNumber, int amount)
        {
            var sourceAccount = await _accountRepository.GetByAccount(sourceAccountNumber);
            var destinationAccount = await _accountRepository.GetByAccount(destinationAccountNumber);

            if (sourceAccount is null) return NotFound();
            if (destinationAccount is null) return NotFound();

            sourceAccount.Withdraw(amount);
            destinationAccount.Deposit(amount);

            await _accountRepository.Update(sourceAccount);
            await _accountRepository.Update(destinationAccount);

            return AcceptedAtAction(nameof(GetUserAccount),
                new { id = sourceAccount.Id },
                sourceAccount);
        }


    }
}
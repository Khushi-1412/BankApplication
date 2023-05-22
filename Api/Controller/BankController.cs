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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankAccount>>> Index()
        {
            var accounts = await _accountRepository.List();
            return Ok(accounts);
        }
      
        [HttpGet("{id}")]
        public async Task<ActionResult<BankAccount?>> GetUser(Guid id)
        {
            var bank =await _accountRepository.GetById(id);
            return bank;
        }
        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Post(CreateBankDTO dto)
        {
            BankAccount bank = new BankAccount(dto.Name, dto.Email, dto.Phone, dto.Address, dto.Balance);
        
            await  _accountRepository.Add(bank);
            return CreatedAtAction(nameof(GetUser),
                new { id = bank.Id }, 
                bank); 
        }
        [HttpPut("{id}/deposit")]
        public async Task<IActionResult> DepositMoney(Guid id, int amount)
        {
            var b = await _accountRepository.GetById(id);
            if (b is null) return NotFound();
            b.Deposit(amount);
            await _accountRepository.Update(b);
            return AcceptedAtAction(nameof(GetUser),
                new { id = b.Id },
                b);
        }
        [HttpPut("{id}/withdraw")]
        public async Task<IActionResult> WithdrawMoney(Guid id, int amount)
        {
            var b =await _accountRepository.GetById(id);
            if (b is null) return NotFound();
            b.Withdraw(amount);
            await _accountRepository.Update(b);
            return AcceptedAtAction(nameof(GetUser),
                new { id = b.Id },
                b);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bank = await _accountRepository.GetById(id);
            if (bank is null) return NotFound();
            await _accountRepository.Delete(bank);
            return NoContent();



        }
    }
}
using System.Reflection.Emit;

namespace BankApplication.Api.DTO
{

    public record CreateBankDTO(
         string AccountHolderName,
         string Email,
         string Phone,
         string Gender,
         int Age,
         string Address,
         decimal AccountBalance
    );

}

namespace BankApplication.Api.DTO
{
   
        public record CreateBankDTO(
            string Name,
            string Email,
            string Phone,
            string Address,
            long AccountNumber,
            int Balance
        );
    
}

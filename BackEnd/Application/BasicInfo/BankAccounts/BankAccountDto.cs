using LawyerProject.Domain.Entities.BankEntities;

namespace LawyerProject.Application.BasicInfo.Banks
{
    public class BankAccountDto
    {
        public int Id { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public int BankBranchId { get; set; } = 0;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<BankAccount, BankAccountDto>();
            }
        }
    }
}

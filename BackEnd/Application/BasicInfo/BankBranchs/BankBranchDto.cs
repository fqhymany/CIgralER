using LawyerProject.Domain.Entities.BankEntities;

namespace LawyerProject.Application.BasicInfo.Banks
{
    public class BankBranchDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int BankId { get; set; } = 0;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<BankBranch, BankBranchDto>();
            }
        }
    }
}

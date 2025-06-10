using LawyerProject.Domain.Entities.BankEntities;

namespace LawyerProject.Application.BasicInfo.Banks
{
    public class BankDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Bank, BankDto>();
            }
        }
    }
}

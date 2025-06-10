using LawyerProject.Domain.Entities.CaseFinancials;

namespace LawyerProject.Application.BasicInfo.PaymentMethodOptions
{
    public class PaymentMethodOptionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<PaymentMethodOption, PaymentMethodOptionDto>();
            }
        }
    }
}

using LawyerProject.Domain.Entities.CaseFinancials;

namespace LawyerProject.Application.BasicInfo.PaymentMethodSchedules
{
    public class PaymentMethodScheduleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<PaymentMethodSchedule, PaymentMethodScheduleDto>();
            }
        }
    }
}

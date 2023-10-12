using test0000001.Models.LifeInsurance;

namespace test0000001.Models.DTO.LifeInsurance
{
    public class PackageOverviewDto
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();

        public Policyholder PolicyHolder { get; set; } = new Policyholder();

        public LifeInsuredObject InsuredObject { get; set; } = new LifeInsuredObject();

        public AppraisalInfosDto AppraisalInfos { get; set; } = new AppraisalInfosDto();

        public AppraisalDossier AppraisalDossier { get; set; } = new AppraisalDossier();

        public Policy? Package {  get; set; }

        public List<PaymentScheduleDto>? Payments { get; set; }
    }
}

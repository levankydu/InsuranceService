using InsuranceServices.Models.LifeInsurance;

namespace InsuranceServices.Models.DTO.LifeInsurance
{
    public class InsuredObjectDto
    {
        public Policyholder PolicyHolder { get; set; } = new Policyholder();

        public LifeInsuredObject InsuredObject { get; set; } = new LifeInsuredObject();

    }
}

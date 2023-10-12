using test0000001.Models.LifeInsurance;

namespace test0000001.Models.DTO.LifeInsurance
{
    public class InsuredObjectDto
    {
        public Policyholder PolicyHolder { get; set; } = new Policyholder();

        public LifeInsuredObject InsuredObject { get; set; } = new LifeInsuredObject();

    }
}

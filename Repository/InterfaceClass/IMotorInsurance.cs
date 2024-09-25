using InsuranceServices.Models;

namespace InsuranceServices.Repository.InterfaceClass
{
    public interface IMotorInsurance
    {

        Task<bool> EditCar(CarInsuredObject editCar);
        Task<bool> DeleteCar(int id);
    }
}

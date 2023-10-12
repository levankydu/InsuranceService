using test0000001.Models;

namespace test0000001.Repository.InterfaceClass
{
    public interface IMotorInsurance
    {

        Task<bool> EditCar(CarInsuredObject editCar);
        Task<bool> DeleteCar(int id);
    }
}

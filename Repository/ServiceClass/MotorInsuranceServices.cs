
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{
   
    public class MotorInsuranceServices : IMotorInsurance
    {
        private readonly DatabaseContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public MotorInsuranceServices(DatabaseContext _db, UserManager<ApplicationUser> _userManager)
        {
            db = _db;
            userManager = _userManager;
        }

        public async Task<bool> DeleteCar(int id)
        {
            var car = await db.CarInsuredObject!.SingleOrDefaultAsync(p => p.Id.Equals(id));
            if (car == null)
            {
                return false;
            }
            db.CarInsuredObject!.Remove(car);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditCar(CarInsuredObject editCar)
        {
            var car = await db.CarInsuredObject!.SingleOrDefaultAsync(u => u.Id!.Equals(editCar.Id));
            if (car != null)
            {
                car.YearsOfManufacture = editCar.YearsOfManufacture;
                car.Automaker = editCar.Automaker;
                car.CarBand = editCar.CarBand;
                car.CarType = editCar.CarType;
                car.CityOfCarReg = editCar.CityOfCarReg;
                await db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

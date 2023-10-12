using test0000001.DB;
using Microsoft.EntityFrameworkCore;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{
    public class InsuranceCategoryServices : IInsuranceCategory
    {
        private DatabaseContext db;

        public InsuranceCategoryServices(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<bool> addCategory(InsuranceCategory newCategory)
        {
            await db.insuranceCategory!.AddAsync(newCategory);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> editCategory(InsuranceCategory editCategory)
        {
            var category = await db.insuranceCategory!.SingleOrDefaultAsync(u => u.Id!.Equals(editCategory.Id));
            if (category != null)
            {
                category.Name = editCategory.Name;
                await db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }

        }

        public async Task<bool> deleteCategory(int id)
        {
            var category = await db.insuranceCategory!.SingleOrDefaultAsync(t => t.Id.Equals(id));
            if (category == null)
            {
                return false;
            }
            db.insuranceCategory!.Remove(category);
            await db.SaveChangesAsync();        
            return true;
        }


        public async Task<InsuranceCategory> GetCategoryById(int id)
        {
            return await db.insuranceCategory!.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<InsuranceCategory>> GetAllCategory()
        {
            return await db.insuranceCategory!.ToListAsync();
        }

        public async Task<bool> IsCategoryUnique(string nameCategory)
        {
            return await db.insuranceCategory!.AllAsync(t => t.Name != nameCategory);
        }
    }
}

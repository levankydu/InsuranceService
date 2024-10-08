﻿using InsuranceServices.Models;

namespace InsuranceServices.Repository.InterfaceClass
{
    public interface IInsuranceCategory
    {
        Task<IEnumerable<InsuranceCategory>> GetAllCategory();
        Task<InsuranceCategory> GetCategoryById(int id);
        Task<bool> addCategory(InsuranceCategory newCategory);
        Task<bool> editCategory(InsuranceCategory editCategory);
        Task<bool> deleteCategory(int id);

        Task<bool> IsCategoryUnique(string nameCategory);
    }
}

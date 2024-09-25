using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using InsuranceServices.DB;
using InsuranceServices.Models;
using InsuranceServices.Repository.InterfaceClass;
using InsuranceServices.Repository.ServiceClass;

namespace InsuranceServices.Controllers
{
    public class InsuranceCategoryController : Controller
    {
        private IInsuranceCategory insuranceCategoryService;
        private DatabaseContext dbContext;

        public InsuranceCategoryController(IInsuranceCategory _insuranceCategoryService,  DatabaseContext _dbContext)
        {
            insuranceCategoryService = _insuranceCategoryService;
            dbContext = _dbContext;

        }
        
        public async Task<IActionResult> Index()
        {
            var model = await insuranceCategoryService.GetAllCategory();
            return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InsuranceCategory newCategory)
        {
            await insuranceCategoryService.addCategory(newCategory);
            return RedirectToAction("Index", "InsuranceCategory");
        }
        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await insuranceCategoryService.GetCategoryById(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InsuranceCategory editCategory)
        {   
            await insuranceCategoryService.editCategory(editCategory);
            TempData["msg"] = "Congratulation !!! Edit Success";
            return RedirectToAction("Index", "InsuranceCategory");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await insuranceCategoryService.GetCategoryById(id);
            var policy = dbContext.Policy!.Where(p => p.InsuranceCategoryId == id).ToList();
            if (category == null || policy.Count > 0)
            {
                TempData["msg"] = "Delete Category Fail";
                return RedirectToAction("Index", "InsuranceCategory");
            }
            else
            {
                await insuranceCategoryService.deleteCategory(id);
                TempData["msg"] = "Delete Category Success";
                return RedirectToAction("Index", "InsuranceCategory");
            }
        }
    }
}



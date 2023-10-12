using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Numerics;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;
using test0000001.Repository.ServiceClass;

namespace test0000001.Controllers
{
    public class PolicyController : Controller
    {
        private IInsuranceCategory insuranceCategoryService;
        private IDuration durationService;
        private IPolicy policyService;
        
        private DatabaseContext dbContext;

        public PolicyController(IPolicy _policyService, IDuration _durationService, IInsuranceCategory _insuranceCategoryService, DatabaseContext _dbContext)
        {
            insuranceCategoryService = _insuranceCategoryService;
            durationService = _durationService;
            policyService = _policyService;
            dbContext = _dbContext;
        }
        public async Task<IActionResult> Index()
        {
            var model = await policyService.GetAllPolicy();
            return View(model);
           
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var insuranceCategory = await insuranceCategoryService.GetAllCategory();
            //ViewBag.TeamId = new SelectList(teams.Select(x => x.Id));
            ViewBag.CategoryId = new SelectList(insuranceCategory, "Id", "Name");
            var duration = await durationService.GetAllDuration();
            ViewBag.DurationId = new SelectList(duration, "Id", "Term");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Policy newPolicy)
        {          
            await policyService.AddPolicy(newPolicy);
            return RedirectToAction("Index", "Policy");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var insuranceCategory = await insuranceCategoryService.GetAllCategory();
            //ViewBag.TeamId = new SelectList(teams.Select(x => x.Id));
            ViewBag.CategoryId = new SelectList(insuranceCategory, "Id", "Name");
            var duration = await durationService.GetAllDuration();
            ViewBag.DurationId = new SelectList(duration, "Id", "Term");

            var model = await policyService.GetPolicyById(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Policy editPolicy)
        {
            await policyService.EditPolicy(editPolicy);
            TempData["msg"] = "Congratulation !!! Edit Success";
            return RedirectToAction("Index", "Policy");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var model = await policyService.DeletePolicy(id);
                TempData["msg"] = "Delete Policy Success";
                return RedirectToAction("Index", "Policy");

            }
            catch (Exception)
            {
                ViewBag.errormsg = "Delete Policy Fail";
                return RedirectToAction("Index", "Policy");
            }
        }
    }
}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using test0000001.Models.LifeInsurance;
using test0000001.Repository.ServiceClass.LifeInsurance;

namespace test0000001.Controllers.Admin
{
    [Route("Admin/[Controller]/[Action]/{id?}")]
    [Authorize(Roles = "admin")]
    public class AppraisalQuesController : Controller
    {
        private readonly AppraisalQuesService _appraisalQues;
        private readonly string _viewPath = "../Admin/LifeInsurance/AppraisalQues";

        public AppraisalQuesController(AppraisalQuesService appraisalQues)
        {
            _appraisalQues = appraisalQues;
        }

        // GET: Admin/AppraisalQues
        public IActionResult Index()
        {
            var model = _appraisalQues.GetByCateId(1);
            return View($"{_viewPath}/Index", model);
        }

        // GET: Admin/AppraisalQues/Create
        public IActionResult Create()
        {
            return View($"{_viewPath}/CreateOrEdit");
        }

        // POST: Admin/AppraisalQues/Create
        [HttpPost]
        public async Task<IActionResult> Create(AppraisalQues appraisalQues)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    appraisalQues.InsuranceCategoryId = 1;
                    await Task.FromResult(_appraisalQues.Add(appraisalQues));
                    return RedirectToAction(nameof(Index));
                }
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", appraisalQues);
            }
        }

        // GET: Admin/AppraisalQues/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await Task.FromResult(_appraisalQues.GetById(id));
                if (model == null) return NotFound();
                return View($"{_viewPath}/CreateOrEdit", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/AppraisalQues/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(int id, AppraisalQues appraisalQues)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await Task.FromResult(_appraisalQues.Edit(appraisalQues));
                    return RedirectToAction(nameof(Index));
                }
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", appraisalQues);
            }
        }

        // GET: Admin/AppraisalQues/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await Task.FromResult(_appraisalQues.GetById(id));
                if (model == null) return NotFound();
                return PartialView($"{_viewPath}/Delete", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/AppraisalQues/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            try
            {
                await Task.FromResult(_appraisalQues.Delete(id));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

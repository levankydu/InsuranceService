using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Models;

namespace test0000001.Controllers
{
    [Route("Admin/[Controller]/[Action]/{id?}")]
    public class DurationController : Controller
    {
        private readonly DatabaseContext _dbContext;
        private readonly string _viewPath = "../Admin/Duration";

        public DurationController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Admin/Duration
        public IActionResult Index()
        {
            var model = _dbContext.Duration.ToList();
            return View($"{_viewPath}/Index", model);
        }

        // GET: Admin/Duration/Create
        public IActionResult Create()
        {
            return View($"{_viewPath}/CreateOrEdit");
        }

        // POST: Admin/Duration/Create
        [HttpPost]
        public async Task<IActionResult> Create(Duration duration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _dbContext.Duration.AddAsync(duration);
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", duration);
            }
        }

        // GET: Admin/Duration/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await _dbContext.Duration.FindAsync(id);
                if (model == null) return NotFound();
                return View($"{_viewPath}/CreateOrEdit", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/Duration/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Duration duration)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _dbContext.Entry(duration).State = EntityState.Modified;
                    await _dbContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", duration);
            }
        }

        // GET: Admin/Duration/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await _dbContext.Duration.FindAsync(id);
                if (model == null) return NotFound();
                return PartialView($"{_viewPath}/Delete", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/Duration/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            try
            {
                var model = await _dbContext.Duration.FindAsync(id);
                if (model == null) return NotFound();
                _dbContext.Remove(model);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}

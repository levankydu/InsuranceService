using Microsoft.AspNetCore.Mvc;
using test0000001.DB;

namespace test0000001.Controllers
{
    [Route("Admin/[Controller]/[Action]/{id?}")]
    public class PolicyHolderController : Controller
    {
        private readonly DatabaseContext _dbContext;
        private readonly string _viewPath = "../Admin/PolicyHolder";

        public PolicyHolderController(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: Admin/PolicyHolder
        public IActionResult Index()
        {
            var model = _dbContext.Policyholder.ToList();
            return View($"{_viewPath}/Index", model);
        }
    }
}

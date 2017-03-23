using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.ViewModels;

namespace PoliticalPurse.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpGet("donations")]
        public IActionResult Donations()
        {
            return View("Data", new DataViewModel("byparty"));
        }

        [HttpGet("parties")]
        public IActionResult Parties()
        {
            return View("Data", new DataViewModel("national"));
        }

        [HttpGet("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}

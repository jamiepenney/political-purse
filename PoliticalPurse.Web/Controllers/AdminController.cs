using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.Services;

namespace PoliticalPurse.Web.Controllers
{
    [Route("admin")]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly DonationUpdateService _updateService;

        public AdminController(DonationUpdateService updateService)
        {
            _updateService = updateService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("donations")]
        public async Task<IActionResult> UpdateDonations(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var result = await _updateService.UpdateDonations(stream);
                return Json(new { result });
            }

            
        }

    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.Services;

namespace PoliticalPurse.Web.Controllers
{
    [Route("donations/{version}")]
    public class DonationController : Controller
    {
        private DonationService _database;

        public DonationController(DonationService database)
        {
            _database = database;
        }

        [HttpGet("")]
        public async Task<ActionResult> List()
        {
            var donations = await _database.GetDonations();
            return Json(donations);
        }

        [HttpGet("search")]
        public async Task<ActionResult> Search(DonationQuery query)
        {
            var donations = await _database.SearchDonations(query);
            return Json(donations);
        }

        [HttpGet("byparty")]
        public async Task<ActionResult> ByParty(DonationQuery query)
        {
            var donations = await _database.GetDonationsByParty(query);
            return Json(new {
                structure = PartyDonations.Structure,
                data = donations
            });
        }

        [HttpGet("bydonee")]
        public async Task<ActionResult> ByDonee(DonationQuery query)
        {
            var donations = await _database.GetDonationsByDoneeAndParty(query);
            return Json(new {
                structure = PartyAndDoneeDonations.Structure,
                data = donations
            });
        }
    }
}
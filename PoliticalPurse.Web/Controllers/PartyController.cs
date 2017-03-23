using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.Services;

namespace PoliticalPurse.Web.Controllers
{
    [Route("party/{version}")]
    public class PartyController : Controller
    {
        private DatabaseService _database;

        public PartyController(DatabaseService database)
        {
            _database = database;
        }

        [HttpGet("search")]
        public async Task<ActionResult> Search(DonationQuery query)
        {
            var donations = await _database.SearchDonations(query);
            return Json(donations);
        }

        [HttpGet("national")]
        public async Task<ActionResult> ByParty(DonationQuery query)
        {
            var donations = await _database.GetDonationsByParty(query);
            return Json(new {
                structure = PartyDonations.Structure,
                data = donations
            });
        }
    }
}
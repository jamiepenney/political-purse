using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PoliticalPurse.Web.Services;

namespace PoliticalPurse.Web.Controllers
{
    [Route("party/{version}")]
    public class PartyController : Controller
    {
        private PartyService _database;

        public PartyController(PartyService database)
        {
            _database = database;
        }

        private async Task<ActionResult> RunQuery(PartyQuery query)
        {
            var data = await _database.GetParty(query);
            return Json(new {
                structure = DonationToParty.Structure,
                data
            });
        }

        [HttpGet("national")]
        public async Task<ActionResult> National(PartyQuery query)
        {
            query.Party = "New Zealand National Party";
            return await RunQuery(query);
        }

        [HttpGet("labour")]
        public async Task<ActionResult> Labour(PartyQuery query)
        {
            query.Party = "New Zealand Labour Party";
            return await RunQuery(query);
        }

        [HttpGet("greens")]
        public async Task<ActionResult> Greens(PartyQuery query)
        {
            query.Party = "Green Party of Aotearoa New Zealand";
            return await RunQuery(query);
        }
    }
}
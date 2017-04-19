using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Dapper;
using System.Linq;
using System.Threading.Tasks;

namespace PoliticalPurse.Web.Services
{
    public class PartyService : DatabaseService
    {
        public PartyService(IOptions<DatabaseOptions> options) : base(options) {}

        public async Task<List<DonationToParty>> GetParty(PartyQuery query)
        {
            using(var connection = GetConnection())
            {
                return (await connection.QueryAsync<DonationToParty>(
                    @"SELECT donee, SUM(amount) as total, SUM(number_of_donations) as numberOfDonations,
                             SUM(amount) / SUM(number_of_donations) as average
                      FROM donation
                      WHERE number_of_donations > 0
                      AND   party = @Party
                      AND   @Year IS NULL or year = @Year
                      GROUP BY donee", query
                )).ToList();
            }
        }
    }

    public class PartyQuery
    {
        public string Party { get; set; }
        public int? Year { get; set; }
        public string TextSearch { get; set; }

        public string Description
        {
            get
            {
                string val;
                if (Year.HasValue)
                {
                    val = "For " + Year;
                }
                else
                {
                    val = "1996 - 2015";
                }

                if (!string.IsNullOrEmpty(TextSearch))
                {
                    val += " containing " + TextSearch;
                }

                return val;
            }
        }
    }

    public class DonationToParty
    {
        public string Donee { get; set; }
        public decimal Average { get; set; }
        public decimal Total { get; set; }
        public int NumberOfDonations { get; set; }

        public static readonly DataDefinition Structure = new DataDefinition
        {
            Name = "Declared Donations to the {{party}}",
            Datatable = new DatatableDefinition {
                InitialSort = "total",
                InitialSortDirection = SortDirection.desc
            },
            Properties = new Structure {
                new StructureElement("donee", "Donee", "string"),
                new StructureElement("total", "Total", "number", "$###,###,###"),
                new StructureElement("average", "Average", "number", "$###,###,###"),
                new StructureElement("numberOfDonations", "Number of Donations", "number"),
            },
            Charts = new List<ChartDefinition> {
                new PieChartDefinition("Total Donations"){
                    Label = "donee", Data = "total"
                },
                new BarChartDefinition("Average Donations"){
                    Label = "donee", Data = "average"
                },
                new BarChartDefinition("Number of Donations"){
                    Label = "donee", Data = "numberOfDonations"
                },
            }
        };
    }
}
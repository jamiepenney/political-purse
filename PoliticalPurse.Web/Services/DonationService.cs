using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Dapper;
using PoliticalPurse.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PoliticalPurse.Web.Services
{
    public class DonationService : DatabaseService
    {
        public DonationService(IOptions<DatabaseOptions> options) : base(options) {}

        public async Task<List<Donation>> GetDonations()
        {
            using(var connection = GetConnection())
            {
                return (await connection.QueryAsync<Donation>(
                    "SELECT * FROM donation"
                )).ToList();
            }
        }

        public async Task<List<Donation>> SearchDonations(DonationQuery query)
        {
            using(var connection = GetConnection())
            {
                return (await connection.QueryAsync<Donation>(
                    @"SELECT * FROM donation
                    WHERE donee like @TextSearch OR party like @TextSearch or donee_address like @TextSearch",
                    query
                )).ToList();
            }
        }

        public async Task<List<PartyDonations>> GetDonationsByParty(DonationQuery query)
        {
            using(var connection = GetConnection())
            {
                return (await connection.QueryAsync<PartyDonations>(
                    @"SELECT party, SUM(amount) as total, SUM(number_of_donations) as numberOfDonations,
                             SUM(amount) / COALESCE(SUM(number_of_donations), COUNT(id), 1) as average
                      FROM donation
                      WHERE number_of_donations > 0
                      AND   @Year IS NULL or year = @Year
                      GROUP BY party", query
                )).ToList();
            }
        }

        public async Task<List<PartyAndDoneeDonations>> GetDonationsByDoneeAndParty(DonationQuery query)
        {
            using(var connection = GetConnection())
            {
                return (await connection.QueryAsync<PartyAndDoneeDonations>(
                    @"SELECT party, donee, SUM(amount) as total, SUM(number_of_donations) as numberOfDonations,
                             SUM(amount) / COALESCE(SUM(number_of_donations), COUNT(id), 1) as average
                      FROM donation
                      WHERE number_of_donations > 0
                      AND   @Year IS NULL or year = @Year
                      GROUP BY party, donee", query
                )).ToList();
            }
        }
    }

    public class DonationQuery
    {
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
                    val = "1996 - 2017";
                }

                if (!string.IsNullOrEmpty(TextSearch))
                {
                    val += " containing " + TextSearch;
                }

                return val;
            }
        }
    }

    public class PartyDonations
    {
        public string Party { get; set; }
        public decimal Total { get; set; }
        public decimal Average { get; set; }
        public int NumberOfDonations { get; set; }

        public static readonly DataDefinition Structure = new DataDefinition()
        {
            Name = "Declared Donations by Party",
            Datatable = new DatatableDefinition {
                InitialSort = "total",
                InitialSortDirection = SortDirection.desc
            },
            Properties = new Structure {
                new StructureElement("party", "Party", "string"),
                new StructureElement("total", "Total", "number", "$###,###,###"),
                new StructureElement("average", "Average", "number", "$###,###,###"),
                new StructureElement("numberOfDonations", "Number of Donations", "number"),
            },
            Charts = new List<ChartDefinition> {
                new PieChartDefinition("Total Donations"){
                    Label = "party", Data = "total"
                },
                new BarChartDefinition("Average Donations"){
                    Label = "party", Data = "average"
                },
                new BarChartDefinition("Number of Donations"){
                    Label = "party", Data = "numberOfDonations"
                },
            },
            Query = new QueryDefinition
            {
                Parameters = new List<QueryParameterDefinition>
                {
                    new QueryParameterDefinition("Year", "year", QueryType.year, false)
                    {
                        MinValue = 1996,
                        MaxValue = 2017
                    }
                }
            }
        };
    }

    public class PartyAndDoneeDonations
    {
        public string Party { get; set; }
        public string Donee { get; set; }
        public string Label => Party + " - " + Donee;
        public decimal Average { get; set; }
        public decimal Total { get; set; }
        public int NumberOfDonations { get; set; }

        public static readonly DataDefinition Structure = new DataDefinition()
        {
            Name = "Declared Donations by Party and Donee",
            Datatable = new DatatableDefinition {
                InitialSort = "total",
                InitialSortDirection = SortDirection.desc,
                SkipProperties = new [] { "label" }
            },
            Properties = new Structure {
                new StructureElement("party", "Party", "string"),
                new StructureElement("donee","Donee", "string"),
                new StructureElement("label","Label", "string"),
                new StructureElement("total","Total", "number", "$###,###,###"),
                new StructureElement("average", "Average", "number", "$###,###,###"),
                new StructureElement("numberOfDonations", "Number of Donations", "number"),
            },

            Charts = new List<ChartDefinition> {
                new BarChartDefinition("Average Donations"){
                    Label = "label", Data = "average"
                },
                new BarChartDefinition("Number of Donations"){
                    Label = "label", Data = "numberOfDonations"
                },
            }
        };
    }
}
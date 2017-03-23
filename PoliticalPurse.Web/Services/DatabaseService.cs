using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;
using Dapper;
using PoliticalPurse.Web.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PoliticalPurse.Web.Services
{
    public class DatabaseService
    {
        private DatabaseOptions _options;

        public DatabaseService(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        private IDbConnection GetConnection()
        {
            var connection = new NpgsqlConnection(BuildConnectionString(_options.DATABASE_URL));
            return connection;
        }

        private string BuildConnectionString(string connectionString)
        {
            if(!connectionString.StartsWith("postgres://")) {
                return connectionString;
            }
            var builder = new NpgsqlConnectionStringBuilder();

            var split = connectionString.Replace("postgres://" ,"").Split(':', '/', '@');

            builder.Username = split[0];
            builder.Password = split[1];
            builder.Host = split[2];
            builder.Port = int.Parse(split[3]);
            builder.Database = split[4];
            builder.Pooling = true;

            return builder.ConnectionString;
        }

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
                             SUM(amount) / SUM(number_of_donations) as average
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
                             SUM(amount) / SUM(number_of_donations) as average
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
        public string Party { get; set; }
        public string TextSearch { get; set; }
    }
    public class DataDefinition
    {
        public string Name { get; set; }
        public Structure Properties { get; set; }
        public DatatableDefinition Datatable { get; set; }
        public List<ChartDefinition> Charts { get; set; }
    }

    public class DatatableDefinition
    {
        public string InitialSort { get; set; }
        public SortDirection InitialSortDirection { get; set; }
        public string[] SkipProperties { get; set; }
    }

    public enum SortDirection { asc, desc }

    public class Structure : List<StructureElement> {}
    public abstract class ChartDefinition
    {
        public abstract string Type { get; }
        public string Title { get; set; }

        public ChartDefinition(string title) {
            Title = title;
        }
    }

    public class PieChartDefinition : ChartDefinition
    {
        public override string Type { get { return "pie"; } }
        public string Label { get; set; }
        public string Data { get; set; }
        public PieChartDefinition(string title) : base(title) {}
    }

    public class BarChartDefinition : ChartDefinition
    {
        public override string Type { get { return "bar"; } }
        public string Label { get; set; }
        public string Data { get; set; }
        public BarChartDefinition(string title) : base(title) {}
    }

    public struct StructureElement
    {
        public StructureElement(string key, string name, string type, string formatter = "")
        {
            Key = key;
            Name = name;
            Type = type;
            Formatter = formatter;
        }
        public string Key { get; }
        public string Name { get; }
        public string Type { get; }
        public string Formatter { get; }

    }
    public class PartyDonations
    {
        public string Party { get; set; }
        public decimal Total { get; set; }
        public decimal Average { get; set; }
        public int NumberOfDonations { get; set; }

        public readonly static DataDefinition Structure = new DataDefinition()
        {
            Name = "Donations by Party",
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
        public readonly static DataDefinition Structure = new DataDefinition()
        {
            Name = "Donations by Party and Donee",
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
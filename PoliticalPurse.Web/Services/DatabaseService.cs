using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace PoliticalPurse.Web.Services
{
    public class DatabaseService
    {
        private readonly DatabaseOptions _options;

        public DatabaseService(IOptions<DatabaseOptions> options)
        {
            _options = options.Value;
        }

        protected IDbConnection GetConnection()
        {
            var connection = new NpgsqlConnection(BuildConnectionString(_options.DATABASE_URL));
            connection.Open();
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
    }

    public class DataDefinition
    {
        public string Name { get; set; }
        public Structure Properties { get; set; }
        public DatatableDefinition Datatable { get; set; }
        public List<ChartDefinition> Charts { get; set; }
        public QueryDefinition Query { get; set; }
    }

    public class QueryDefinition
    {
        public List<QueryParameterDefinition> Parameters { get; set; }
    }

    public enum QueryType { year }
    public class QueryParameterDefinition
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public QueryType Type { get; set; }
        public bool Optional { get; set; }
        public object MinValue { get; set; }
        public object MaxValue { get; set; }

        public QueryParameterDefinition(string title, string name, QueryType type, bool optional)
        {
            Title = title;
            Name = name;
            Type = type;
            Optional = optional;
        }
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

        protected ChartDefinition(string title) {
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
}
namespace PoliticalPurse.Web.ViewModels
{
    public class DataViewModel
    {
        public string Datasource { get; set; }
        public string DefaultQuery { get; set; }

        public DataViewModel(string datasource, string defaultQuery)
        {
            Datasource = datasource;
            DefaultQuery = defaultQuery;
        }
    }
}
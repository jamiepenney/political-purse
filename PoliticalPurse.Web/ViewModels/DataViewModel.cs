namespace PoliticalPurse.Web.ViewModels
{
    public class DataViewModel
    {
        public string DefaultDatasource { get; set; }

        public DataViewModel(string defaultDatasource)
        {
            DefaultDatasource = defaultDatasource;
        }
    }
}
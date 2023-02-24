namespace TMS.DataContracts.HistoricalModels
{
    public class Version
    {
        public int Id { get; set; }
        public FactType FactType { get; set; }
        public string Hash { get; set; }

    }
}
namespace TMS.DataContracts.HistoricalModels
{
    public class Fact
    {
        public int Id { get; set; }
        public int VersionId { get; set; }
        public string Hash { get; set; }
        public string Fields { get; set; }
    }
}

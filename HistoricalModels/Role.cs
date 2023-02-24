namespace TMS.DataContracts.HistoricalModels
{
    public class Role
    {
        public int Id { get; set; }
       // public FactType DeclaringType { get; set; }
        public int DeclaringTypeId { get; set; }
       // public FactType TargetType { get; set; }
        public int TargetTypeId { get; set; }
        public string Name { get; set; }

    }
}
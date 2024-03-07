namespace EntityEndpoint.API.Models
{
    public class Entity : IEntity
    {
      public List<Address>? Addresses { get; set; }
        public required List<Date> Dates { get; set; }
        public bool Deceased { get; set; }
        public string? Gender { get; set; }
        public required string Id { get; set; }
        public required List<Name> Names { get; set; }
    }
}

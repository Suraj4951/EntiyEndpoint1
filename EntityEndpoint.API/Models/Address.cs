using System.ComponentModel.DataAnnotations;

namespace EntityEndpoint.API.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}

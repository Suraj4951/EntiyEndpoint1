using System.ComponentModel.DataAnnotations;

namespace EntityEndpoint.API.Models
{
    public class Date
    {
        [Key]
        public int Id { get; set; }
        public string? DateType { get; set; }
        public DateTime? DateTime { get; set; }
    }
}

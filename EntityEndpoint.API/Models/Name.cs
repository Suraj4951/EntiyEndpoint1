using System.ComponentModel.DataAnnotations;

namespace EntityEndpoint.API.Models
{
    public class Name
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Surname { get; set; }
    }
}

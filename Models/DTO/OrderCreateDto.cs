using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models.DTO
{
    public class OrderCreateDto
    {
        [Required]
        public string CustomerName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string CustomerEmail { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string CustomerPhoneNumber { get; set; }
        [Required]
        public DateTime DeadLine { get; set; }
        public string Notes { get; set; }
        [Required]
        public int PageCount { get; set; }
        public int WordCount { get; set; }
        [Required]
        public string PreferredContact { get; set; }
        [Required]
        public List<string> Services { get; set; }
        [Required]
        public string SourceLanguage { get; set; }
        [Required]
        public string TargetLanguage { get; set; }
        public IFormFile File { get; set; }
    }
}


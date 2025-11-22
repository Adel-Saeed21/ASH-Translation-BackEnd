using ASH_Translation.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public DateTime DeadLine { get; set; }
        public string Notes { get; set; }
        public int PageCount { get; set; }
        public int WordCount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PreferredContact PreferredContact { get; set; }
        public List<string> Services { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public string UploadedFilePath { get; set; }

    }
}

using ASH_Translation.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models
{
    public class Order
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Customer Name is Required")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "Customer Email is Required")]
        [DataType(DataType.EmailAddress,ErrorMessage ="Invalid Email")]
        public string CustomerEmail { get; set; }
        [Required(ErrorMessage = "Customer Phone Number is Required")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "Invalid Phone Number")]
        public string CustomerPhoneNumber { get; set; }
        [Required(ErrorMessage = "DeadLine is Required")]
        public DateTime DeadLine { get; set; }
        public string Notes { get; set; }
        [Required(ErrorMessage = "Pages' Count is Required")]
        public int PageCount { get; set; }
        public int WordCount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        [Required(ErrorMessage = "Preferred Contact is Required")]
        public PreferredContact PreferredContact { get; set; }
        [Required(ErrorMessage ="Services are required at least one")]
        public List<string> Services { get; set; }
        [Required(ErrorMessage = "Source Language is required at least one")]
        public string SourceLanguage { get; set; }
        [Required(ErrorMessage = "Target Language is required at least one")]
        public string TargetLanguage { get; set; }
    }
}

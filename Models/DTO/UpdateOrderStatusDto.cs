using ASH_Translation.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASH_Translation.Models.DTO
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Order Status is Required")]
        public OrderStatus OrderStatus { get; set; }
    }
}


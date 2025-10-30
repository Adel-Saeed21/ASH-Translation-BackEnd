using ASH_Translation.Data;
using ASH_Translation.Models;
using ASH_Translation.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace ASH_Translation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AdminUser> _userManager;

        public OrderController(AppDbContext context,UserManager<AdminUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpPost("/makeOrder")]
        [AllowAnonymous]
        public async Task<IActionResult>MakeOrder([FromBody] Order order)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false,"Invalid Data",errors:ModelState);
                return BadRequest(response);
            }
            // Set default order status to Pending for new orders
            order.OrderStatus = Models.Enums.OrderStatus.Pending;
            var result = await _context.AddAsync(order);
            if (result == null)
            {
                response.SetResponse(false, "UnExpected Error while saving the order");
                return BadRequest(response);
            }
           var IsSaved = await _context.SaveChangesAsync();
            if (IsSaved == 0)
            {
                response.SetResponse(false, "UnExpected Error while saving the order");
                return BadRequest(response);
            }
            response.SetResponse(true,"the Order saved correctly",order);
            return Ok(response);
        }
        [HttpPost("/orders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrder()
        {
            var response = new GeneralResponse();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                response.SetResponse(false, "Unauthorized");
                return Unauthorized(response);
            }
            var user  =await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.SetResponse(false, "Unauthorized");
                return Unauthorized(response);
            }
            var orders = _context.Orders.ToList();
            response.SetResponse(true,"Orderes retrived succesfully",orders);
            return Ok(response);
        }

        [HttpPatch("/orders/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, UpdateOrderStatusDto updateDto)
        {
            var response = new GeneralResponse();
            
            // Validate model
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data", errors: ModelState);
                return BadRequest(response);
            }

           
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                response.SetResponse(false, "Unauthorized");
                return Unauthorized(response);
            }
            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.SetResponse(false, "Unauthorized");
                return Unauthorized(response);
            }

            // Find the order
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                response.SetResponse(false, "Order not found");
                return NotFound(response);
            }

            // Update the order status
            order.OrderStatus = updateDto.OrderStatus;
            
            // Save changes
            var rowsAffected = await _context.SaveChangesAsync();
            if (rowsAffected == 0)
            {
                response.SetResponse(false, "Failed to update order status");
                return BadRequest(response);
            }

            response.SetResponse(true, "Order status updated successfully", order);
            return Ok(response);
        }
    }
}

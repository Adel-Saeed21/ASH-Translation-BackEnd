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
using ASH_Translation.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ASH_Translation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AdminUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public OrderController(AppDbContext context,UserManager<AdminUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }
        [HttpPost("/makeOrder")]
        [AllowAnonymous]
        public async Task<IActionResult>MakeOrder([FromForm] OrderCreateDto dto)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false,"Invalid Data",errors:ModelState);
                return BadRequest(response);
            }
            var order = new Order
            {
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhoneNumber = dto.CustomerPhoneNumber,
                DeadLine = dto.DeadLine,
                Notes = dto.Notes,
                PageCount = dto.PageCount,
                WordCount = dto.WordCount,
                PreferredContact = Enum.Parse<PreferredContact>(dto.PreferredContact, true),
                Services = dto.Services ?? new List<string>(),
                SourceLanguage = dto.SourceLanguage,
                TargetLanguage = dto.TargetLanguage,
                OrderStatus = OrderStatus.Pending
            };
            if (dto.File != null && dto.File.Length > 0)
            {
                var uploadsRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
                if (!Directory.Exists(uploadsRoot))
                {
                    Directory.CreateDirectory(uploadsRoot);
                }
                var safeFileName = Path.GetFileName(dto.File.FileName);
                var uniqueName = $"{Guid.NewGuid()}_{safeFileName}";
                var fullPath = Path.Combine(uploadsRoot, uniqueName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }
                order.UploadedFilePath = $"/uploads/{uniqueName}";
            }

            var result = await _context.AddAsync(order);
            if (result == null)
            {
                response.SetResponse(false, "UnExpected Error while saving the order");
                return BadRequest(response);
            }
           var IsSaved = await _context.SaveChangesAsync();
            if (IsSaved == 0)
            {
                response.SetResponse(false, "Failed to save the order");
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
        
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                response.SetResponse(false, "Order not found");
                return NotFound(response);
            }
            order.OrderStatus = updateDto.OrderStatus;

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

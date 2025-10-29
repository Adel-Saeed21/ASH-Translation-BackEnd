using ASH_Translation.Data;
using ASH_Translation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        public async Task<IActionResult>MakeOrder(Order order)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false,"Invalid Data",errors:ModelState);
                return BadRequest(response);
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
                response.SetResponse(false, "UnExpected Error while saving the order");
                return BadRequest(response);
            }
            response.SetResponse(true,"the Order saved correctly",order);
            return Ok(response);
        }
        [HttpPost("/orders")]
        [Authorize]
        public async Task<IActionResult> GetOrder()
        {
            var response = new GeneralResponse();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
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
    }
}

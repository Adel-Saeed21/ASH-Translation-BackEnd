using ASH_Translation.Data;
using ASH_Translation.Models;
using ASH_Translation.Models.DTO;
using ASH_Translation.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Cmp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASH_Translation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly UserManager<AdminUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        public AccountController(SignInManager<AdminUser> signInManager, UserManager<AdminUser> userManager,
            IConfiguration config, IEmailService emailService, AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _emailService = emailService;
            _context = context;
        }
        //for test //
        [HttpPost("register")]
        public async Task<IActionResult>Register(LoginDto loginDto)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data");
                return BadRequest(response);
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user!=null)
            {
                response.SetResponse(false, "this email already registerd");
                return BadRequest(response);
            }
            var admin = new AdminUser
            {
                FullName = "awwad",
                Email = loginDto.Email,
                UserName = loginDto.Email.Split('@')[0],
            };
            var result = await _userManager.CreateAsync(admin, loginDto.Password);
            if (result.Succeeded)
            {
                response.SetResponse(true, "user registerd !!");
                return Created("", response);
            }
            var errors = new List<string>();
            foreach (var error in result.Errors)
            {
                errors.Add(error.Description);
            }
            response.SetResponse(false, "unexpected error happened!!",errors);
            return BadRequest(response);
        }
        [HttpPost("login")]
        public async Task<IActionResult>Login(LoginDto loginD)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "invalid Data");
                return BadRequest(response);
            }
            var user = await _userManager.FindByEmailAsync(loginD.Email);
            if (user != null)
            {
                var isValid = await _userManager.CheckPasswordAsync(user, loginD.Password);
                if (isValid)
                {
                    var Claims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Email, loginD.Email),
                        new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier,user.Id)
                    };
                    var securitkey = _config["JWT:SecurityKey"];
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securitkey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var token = new JwtSecurityToken(
                     claims:Claims,
                     signingCredentials: creds,
                     expires: DateTime.Now.AddMinutes(30)
                     );
                    var loginToken = new JwtSecurityTokenHandler().
                        WriteToken(token);
                    response.SetResponse(true, "successful Authentication", loginToken);
                    return Ok(response);
                }
            }
            response.SetResponse(false,"Invalid Email or Password");
            return Unauthorized(response);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotDto)
        {
            var response = new GeneralResponse();
            var user = await _userManager.FindByEmailAsync(forgotDto.Email);
            if (user == null)
            {
                response.SetResponse(true, "If your email exists, an OTP will be sent.");
                return Ok(response);
            }
            var otp = new Random().Next(100000, 999999).ToString();

            var otpEntry = new PasswordResetOtp
            {
                Email = forgotDto.Email,
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _context.PasswordResetOtps.Add(otpEntry);
            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(forgotDto.Email, "Password Reset OTP",
                $"Your OTP is <b>{otp}</b>. It expires in 10 minutes.");

            response.SetResponse(true, "If your email exists, an OTP will be sent.");
            return Ok(response);
     
        }
        [HttpPost("verify-otp-reset-password")]
        public async Task<IActionResult> VerifyOtpResetPassword(VerifyOtpResetPasswordDto verifyDto)
        {
            var response = new GeneralResponse();
            var otpRecord = await _context.PasswordResetOtps
                .Where(o => o.Email == verifyDto.Email && o.Otp == verifyDto.Otp && !o.IsUsed)
                .OrderByDescending(o => o.ExpirationTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.ExpirationTime < DateTime.UtcNow)
            {
                response.SetResponse(false, "Invalid or expired OTP.");
                return BadRequest(response);
            }
            var user = await _userManager.FindByEmailAsync(verifyDto.Email);
            if (user == null)
            {
                response.SetResponse(false, "User not found.");
                return BadRequest(response);
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, verifyDto.NewPassword);

            if (!result.Succeeded)
            {
                response.SetResponse(false, "Uexpected Error happened.",result.Errors);
                return BadRequest(response);
            }
       

            otpRecord.IsUsed = true;
            await _context.SaveChangesAsync();
            response.SetResponse(true, "Password reset successfully.");
            return Ok(response);
        }


    }
}

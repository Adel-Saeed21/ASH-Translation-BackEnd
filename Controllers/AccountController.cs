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
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DotNetEnv;

namespace ASH_Translation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AdminUser> _signInManager;
        private readonly UserManager<AdminUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        public AccountController(SignInManager<AdminUser> signInManager, UserManager<AdminUser> userManager,
            IConfiguration config, IEmailService emailService, AppDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _emailService = emailService;
            _context = context;
        }
        //for test //
        [HttpPost("register")]
        public async Task<IActionResult>Register(RegisterDTO registerDto)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data");
                return BadRequest(response);
            }
            var user = await _userManager.FindByEmailAsync(registerDto.Email);
            if(user!=null)
            {
                response.SetResponse(false, "this email already registerd");
                return BadRequest(response);
            }
            var admin = new AdminUser
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                UserName = registerDto.Email.Split('@')[0],
            };
            var result = await _userManager.CreateAsync(admin, registerDto.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await _userManager.AddToRoleAsync(admin, "Admin");
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
            Env.Load();
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
                        new Claim(ClaimTypes.NameIdentifier,user.Id),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var securitkey =  Environment.GetEnvironmentVariable("SecurityKey");
                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securitkey));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    // var issuer = Environment.GetEnvironmentVariable("Issuer");
                    // var audience = Environment.GetEnvironmentVariable("audience");
                    var token = new JwtSecurityToken(
                        // issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                        // audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                        claims: Claims,
                        notBefore: DateTime.UtcNow,
                        expires: DateTime.UtcNow.AddMinutes(30),
                        signingCredentials: creds
                    );
                    var loginToken = new JwtSecurityTokenHandler().
                        WriteToken(token);

                   
                    var refreshToken = GenerateRefreshToken();
                    var refreshTokenEntity = new RefreshToken
                    {
                        Token = refreshToken,
                        UserId = user.Id,
                        ExpiresAt = DateTime.UtcNow.AddDays(7),
                        CreatedAt = DateTime.UtcNow,
                        IsRevoked = false
                    };

                    _context.RefreshTokens.Add(refreshTokenEntity);
                    await _context.SaveChangesAsync();

                  
                    var tokenResponse = new TokenResponseDto
                    {
                        AccessToken = loginToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30)
                    };

                    response.SetResponse(true, "successful Authentication", tokenResponse);
                    return Ok(response);
                }
            }
            response.SetResponse(false,"Invalid Email or Password");
            return Unauthorized(response);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var response = new GeneralResponse();
            
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data", errors: ModelState);
                return BadRequest(response);
            }

            
            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

            if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                response.SetResponse(false, "Invalid or expired refresh token");
                return Unauthorized(response);
            }

            
            var user = await _userManager.FindByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                response.SetResponse(false, "User not found");
                return Unauthorized(response);
            }

            refreshToken.IsRevoked = true;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var securityKey = _config["JWT:SecurityKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _config["JWT:Issuer"];
            var audience = _config["JWT:Audience"];

            var token = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var newRefreshTokenValue = GenerateRefreshToken();
            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshTokenValue,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();
            var tokenResponse = new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            response.SetResponse(true, "Token refreshed successfully", tokenResponse);
            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("OtpPolicy")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotDto)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data");
                return BadRequest(response);
            }
            var user = await _userManager.FindByEmailAsync(forgotDto.Email);
            if (user == null)
            {
                response.SetResponse(true, "If your email exists, an OTP will be sent.");
                return Ok(response);
            }
            var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var otpEntry = new PasswordResetOtp
            {
                Email = forgotDto.Email,
                Otp = otp,
                ExpirationTime = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            _context.PasswordResetOtps.Add(otpEntry);
            await _context.SaveChangesAsync();

            var subject = "ASH Translation - Password Reset Code";
            var body = $@"
                <div style='font-family:Segoe UI,Arial,sans-serif; font-size:14px; color:#1f2937;'>
                    <div style='max-width:560px; margin:0 auto; padding:24px; border:1px solid #e5e7eb; border-radius:8px;'>
                        <h2 style='margin:0 0 16px 0; color:#111827;'>ASH Translation</h2>
                        <p>We received a request to reset the password for your ASH Translation account.</p>
                        <p>Please use the following one-time verification code to continue:</p>
                        <div style='margin:16px 0; padding:16px; background:#f9fafb; border:1px dashed #d1d5db; border-radius:6px; text-align:center;'>
                            <span style='font-size:22px; letter-spacing:4px; font-weight:700; color:#111827;'>{otp}</span>
                        </div>
                        <p>This code will expire in <strong>10 minutes</strong>.</p>
                        <p>If you did not request a password reset, you can safely ignore this email.</p>
                        <p style='margin-top:24px;'>Regards,<br/>The ASH Translation Team</p>
                    </div>
                    <p style='margin-top:12px; font-size:12px; color:#6b7280;'>This is an automated message. Please do not reply.</p>
                </div>";

            await _emailService.SendEmailAsync(forgotDto.Email, subject, body);

            response.SetResponse(true, "If your email exists, an OTP will be sent.");
            return Ok(response);
     
        }
        [HttpPost("verify-otp-reset-password")]
        [EnableRateLimiting("OtpPolicy")]
        public async Task<IActionResult> VerifyOtpResetPassword(VerifyOtpResetPasswordDto verifyDto)
        {
            var response = new GeneralResponse();
            if (!ModelState.IsValid)
            {
                response.SetResponse(false, "Invalid Data");
                return BadRequest(response);
            }
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

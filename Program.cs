
using ASH_Translation.Data;
using ASH_Translation.Models;
using ASH_Translation.Services;
using ASH_Translation.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASH_Translation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var constr = builder.Configuration.GetConnectionString("Constr");
            builder.Services.AddDbContext<AppDbContext>(option=>
            {
                option.UseNpgsql(constr);
            });
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(
                options => 
                { 
                  options.SuppressModelStateInvalidFilter = false;
                });
            builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric =true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            builder.Services.AddScoped<IEmailService, SmtpEmailService>();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthorization();


            app.MapControllers();
            app.UseCors("MyPolicy");
            app.Run();
        }
    }
}

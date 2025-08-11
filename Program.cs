using Car_Rent_System.Models;
using CloudinaryDotNet;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Car_Rent_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Add MVC + API controllers
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            // ✅ Load .env environment variables
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

            // ✅ Database Context
            builder.Services.AddDbContext<CynexBlazerContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Cloudinary setup
            builder.Services.AddSingleton(x =>
            {
                var cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
                cloudinary.Api.Secure = true;
                return cloudinary;
            });

            // ✅ Session configuration
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "CynexBlazer.Session";
            });

            // ✅ JWT Authentication
            var jwtKey = builder.Configuration["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ✅ Build the app
            var app = builder.Build();

            // ✅ Configure middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); // ✅ Middleware for Session (after UseRouting)
            app.UseAuthentication();
            app.UseAuthorization();

            // ✅ Map API routes like /api/accountapi/login
            app.MapControllers();

            // ✅ Map MVC routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ✅ Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<CynexBlazerContext>();
                context.Database.EnsureCreated();
            }

            // ✅ Run the application
            app.Run();
        }
    }
}

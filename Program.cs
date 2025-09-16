using System.Numerics;
using System.Text;
using Car_Rent_System.Data;
using Car_Rent_System.Enums;
using Car_Rent_System.Interfaces;
using Car_Rent_System.Mappers;
using Car_Rent_System.Models;
using Car_Rent_System.Repositories;
using Car_Rent_System.Services;
using Car_Rent_System.Services.Interfaces;
using CloudinaryDotNet;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;


namespace Car_Rent_System
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ Add MVC + API controllers
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            // ✅ Load .env environment variables
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));

            // ✅ Database Context — NOW USING ApplicationDbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ✅ Identity Services — REQUIRED FOR UserManager<ApplicationUser>
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>() // 👈 MUST HAVE THIS
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // ✅ Register IEmailService with credentials from .env
            builder.Services.AddScoped<IEmailService>(provider =>
            {
                var username = Environment.GetEnvironmentVariable("EMAIL_USERNAME");
                var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    throw new InvalidOperationException("EMAIL_USERNAME or EMAIL_PASSWORD is not set in .env file.");
                }

                return new EmailService(username, password);
            });

            // ✅ Scoped Services
            builder.Services.AddScoped<ICarService, CarService>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStripeService, StripeService>();
            builder.Services.AddScoped<IDriverService, DriverService>();
            builder.Services.AddScoped<IOwnerService, OwnerService>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));

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

            // ✅ Authentication - Let Identity handle default cookie authentication
            var jwtKey = builder.Configuration["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication()
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

            // ✅ ADD ROLE-BASED AUTHORIZATION POLICIES
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SubAdmin", "Staff"));
                options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
                options.AddPolicy("DriverOnly", policy => policy.RequireRole("Driver"));
                options.AddPolicy("OwnerOnly", policy => policy.RequireRole("VehicleOwner"));
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

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // ✅ Ensure database is created
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.EnsureCreated();

                // ✅ Seed all roles first
                foreach (var roleName in Enum.GetNames(typeof(Role)))
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // ✅ Get admin email & password from .env (NO fallbacks!)
                var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

                if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                {
                    throw new InvalidOperationException("ADMIN_EMAIL and ADMIN_PASSWORD must be set in .env file.");
                }

                // ✅ Find or create admin user
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        FirstName = "System",
                        LastName = "Administrator",
                        Role = Role.Admin,
                        VerificationStatus = VerificationStatus.Approved,
                        JoinDate = DateTime.UtcNow,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        // ✅ Add user to Admin role (this creates entry in AspNetUserRoles)
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"✅ Admin user created and added to Admin role: {adminEmail}");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"❌ Admin creation failed: {error.Description}");
                        }
                    }
                }
                else
                {
                    // ✅ If user exists, ensure they are in Admin role
                    var userRoles = await userManager.GetRolesAsync(adminUser);
                    if (!userRoles.Contains("Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"✅ Admin user updated: Added to Admin role");
                    }

                    // ✅ Also verify verification status
                    if (adminUser.VerificationStatus != VerificationStatus.Approved)
                    {
                        adminUser.VerificationStatus = VerificationStatus.Approved;
                        await userManager.UpdateAsync(adminUser);
                    }
                }
            }
            app.Run();
        }
    }
}


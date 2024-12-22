using eTickets.Cart;
using eTickets.Controllers;
using eTickets.Hubs;
using eTickets.Models;
using eTickets.Repository;
using eTickets.Services;
using FluentAssertions.Common;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eTickets
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<ikvm.runtime.Startup>();
        })
        .ConfigureServices((hostContext, services) =>
        {
            // إضافة DbInitializer إلى الـ Dependency Injection Container
            services.AddScoped<DbInitializer>();
        });
        public static void Main(string[] args)
        {



            var builder = WebApplication.CreateBuilder(args);

            // تسجيل IMovieRepository مع MovieRepository فقط
            builder.Services.AddScoped<IMovieRepository, MovieRespository>();
            builder.Services.AddScoped<IOtherService, OtherService>();

            builder.Services.AddControllersWithViews();

            // Retrieve IConfiguration from the builder
            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString")));

            // CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });



            // Repositories
            builder.Services.AddScoped<IHeroRepository, HeroRepository>();
            builder.Services.AddScoped<IProducerRepository, ProducerRepository>();
            builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<ILogoRepository, LogoRepository>();

            // HttpContextAccessor
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddScoped(sc => ShoppingCart.GetShoppingCart(sc));

            // Identity configuration
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // السماح بأي حروف في اسم المستخدم
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ءاأإآبتثجحخدذرزسشصضطظعغفقكلمنهويىء";
                options.User.RequireUniqueEmail = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            // MemoryCache and Session
            builder.Services.AddMemoryCache();
            builder.Services.AddSession();

            // Authentication configuration
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            // Add controllers and views
            builder.Services.AddControllersWithViews();

            builder.Services.AddSignalR();

            // Get hosting environment
            var env = builder.Services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

            builder.Services.AddSingleton<IWebHostEnvironment>(env); // تأكد من إضافة IWebHostEnvironment

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Movies/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Movies}/{action=Welcome}/{id?}");
                endpoints.MapHub<ShoppingCartHub>("/shoppingCartHub");
            });

            app.Run();
            var host = CreateHostBuilder(args).Build();

            // استدعاء دالة التهيئة بعد بدء السيرفر
            using (var scope = host.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
                dbInitializer.AddSeatsForExistingMovies().Wait();
            }

            host.Run();
        }
    }

    }



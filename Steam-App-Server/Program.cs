using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Repositories.Interfaces;
using SteamAppServer.Repositories;
using SteamAppServer.Services.Interfaces;
using SteamAppServer.Services;

namespace SteamAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Service Scopes

            builder.Services.AddHttpClient<ISteamService, SteamService>();
            builder.Services.AddScoped<ISalesRepository, SalesRepository>();
            builder.Services.AddScoped<ISteamService, SteamService>();

            #endregion

            // Add CORS configuration
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularClient",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200") // Angular development server
                               .AllowAnyHeader() // Allow any headers (like Authorization)
                               .AllowAnyMethod() // Allow GET, POST, PUT, DELETE
                               .AllowCredentials(); // Allow credentials if needed
                    });
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use CORS in the application
            app.UseCors("AllowAngularClient");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

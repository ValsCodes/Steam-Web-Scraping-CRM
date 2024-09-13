using Microsoft.EntityFrameworkCore;
using SteamAppServer.Context;
using SteamAppServer.Repositories;
using SteamAppServer.Repositories.Interfaces;
using SteamAppServer.Services;
using SteamAppServer.Services.Interfaces;

namespace SteamAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region Service Scopes

            builder.Services.AddHttpClient<ISteamService, SteamService>();
            builder.Services.AddScoped<ISteamRepository, SteamRepository>();
            builder.Services.AddScoped<ISteamService, SteamService>();

            #endregion

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

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

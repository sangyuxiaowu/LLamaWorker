
using LLamaWorker.Middleware;
using LLamaWorker.Models;
using LLamaWorker.Services;
using Microsoft.Extensions.Options;

namespace LLamaWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<Models.LLmModelSettings>(
                builder.Configuration.GetSection("LLmModel")
            );

            builder.Services.AddSingleton<LLmModelService>();

            // 跨域配置
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowCors",
                    policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                );
            });

            var app = builder.Build();

            app.UseCors();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            // 处理 stop 参数
            app.UseMiddleware<StopConversionMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}

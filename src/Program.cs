
using LLamaWorker.Middleware;
using LLamaWorker.Models;
using LLamaWorker.Services;
using Microsoft.Extensions.Options;
using System.Reflection;

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
            builder.Services.AddSwaggerGen(options =>
            {
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });

            builder.Services.Configure<List<LLmModelSettings>>(
                builder.Configuration.GetSection(nameof(LLmModelSettings))
            );
            // 自动释放配置
            GlobalSettings.AutoReleaseTime = builder.Configuration.GetValue<int>("AutoReleaseTime");

            builder.Services.AddSingleton<LLmModelService>();

            // 跨域配置
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowCors",
                    policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
                );
            });

            // HttpClient
            builder.Services.AddHttpClient();

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
            app.UseMiddleware<TypeConversionMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}

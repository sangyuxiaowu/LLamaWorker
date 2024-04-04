
using LLamaWorker.Services;

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

            // ¿çÓòÅäÖÃ
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


            app.MapControllers();

            app.Run();
        }
    }
}

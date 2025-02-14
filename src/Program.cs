
using LLamaWorker.Config;
using LLamaWorker.FunctionCall;
using LLamaWorker.Middleware;
using LLamaWorker.Services;
using Microsoft.OpenApi.Models;
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

            // 配置服务
            builder.Services.Configure<List<LLmModelSettings>>(
                builder.Configuration.GetSection(nameof(LLmModelSettings))
            );
            builder.Services.Configure<List<ToolPromptConfig>>(
                builder.Configuration.GetSection(nameof(ToolPromptConfig))
            );
            // 初始化配置
            GlobalSettings.InitializeGlobalSettings(builder.Configuration);
            // ApiKey 配置
            var apiKey = builder.Configuration.GetValue<string>("ApiKey");


            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LLamaWorker",
                    Version = "v1",
                    Description = "LLamaWorker API",
                    License = new OpenApiLicense
                    {
                        Name = "Apache License 2.0",
                        Url = new System.Uri("https://github.com/sangyuxiaowu/LLamaWorker/blob/main/LICENSE.txt")
                    },
                    TermsOfService = new System.Uri("https://github.com/sangyuxiaowu/LLamaWorker"),
                });

                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                // 添加模型注释
                var xmlModelPath = Path.Combine(AppContext.BaseDirectory, "LLamaWorker.OpenAIModels.xml");
                options.IncludeXmlComments(xmlModelPath);

                if (string.IsNullOrEmpty(apiKey)) return;

                var securityScheme = new OpenApiSecurityScheme()
                {
                    Description = "API Key Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {API_KEY}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                };
                options.AddSecurityDefinition("API_KEY", securityScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "API_KEY"
                            }
                        },
                        new string[] { }
                    }
                });
            });


            builder.Services.AddSingleton<ILLmModelService, LLmModelDecorator>();
            builder.Services.AddSingleton<ToolPromptGenerator>();

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

            // 若存在 wwwroot 目录，则使用静态文件
            if (Directory.Exists("wwwroot"))
            {
                app.UseDefaultFiles();
                app.UseStaticFiles();
            }

            if (!string.IsNullOrEmpty(apiKey))
            {
                app.Use(async (context, next) =>
                {
                    var found = context.Request.Headers.TryGetValue("Authorization", out var key);
                    // 不存在，尝试取一下 api-key
                    if (!found)
                    {
                        found = context.Request.Headers.TryGetValue("api-key", out key);
                    }

                    key = key.ToString().Split(" ")[^1];

                    if (found && key == apiKey)
                    {
                        await next(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                });
            }

            // 处理 stop 参数
            app.UseMiddleware<TypeConversionMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}

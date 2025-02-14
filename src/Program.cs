
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

            // ���÷���
            builder.Services.Configure<List<LLmModelSettings>>(
                builder.Configuration.GetSection(nameof(LLmModelSettings))
            );
            builder.Services.Configure<List<ToolPromptConfig>>(
                builder.Configuration.GetSection(nameof(ToolPromptConfig))
            );
            // ��ʼ������
            GlobalSettings.InitializeGlobalSettings(builder.Configuration);
            // ApiKey ����
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
                // ���ģ��ע��
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

            // ��������
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

            // ������ wwwroot Ŀ¼����ʹ�þ�̬�ļ�
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
                    // �����ڣ�����ȡһ�� api-key
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

            // ���� stop ����
            app.UseMiddleware<TypeConversionMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}

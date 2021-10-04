using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FakeApiEasy
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                builder =>
                                {
                                    builder
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowAnyOrigin();
                                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(MyAllowSpecificOrigins);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/api/v1/{*route}", async context =>
                {
                    await HandleRequestAsync("get", context);
                });

                endpoints.MapPut("/api/v1/{*route}", async context =>
                {
                    await HandleRequestAsync("put", context);
                });

                endpoints.MapPost("/api/v1/{*route}", async context =>
                {
                    await HandleRequestAsync("post", context);
                });

                endpoints.MapDelete("/api/v1/{*route}", async context =>
                {
                    await HandleRequestAsync("delete", context);
                });
            });
        }

        public async Task HandleRequestAsync(string httpmethod, HttpContext context)
        {
            object route = null;
            string filePath = null;
            var fileName = new StringBuilder();

            try
            {
                route = context.Request.RouteValues["route"];

                fileName.Append($"{httpmethod}.{route.ToString().Replace('/', '.')}");

                if (context.Request.QueryString.HasValue)
                {
                    fileName.Append(context.Request.QueryString.Value.Replace('?', '.').Replace('=', '.'));
                }

                fileName.Append(".json");

                filePath = Path.Combine(Environment.CurrentDirectory, "Responses", fileName.ToString());

                await context.Response.WriteAsync(File.ReadAllText(filePath));
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync($"BIG TIME ERROR!!. HttpMethod: {httpmethod.ToUpper()}. Route: {route}. FilePath: {filePath}. Error message: {ex.Message} StackTrace: {ex.StackTrace}");
            }
        }
    }
}

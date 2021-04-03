using Inventart.Config;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventart
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private static string[] AllowedOrigins;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            AllowedOrigins = Configuration.GetValue<string>("AllowedOrigins").Split(";");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "inventart_api", Version = "v1" });
            });

            //configuration
            services.Configure<PostgresConfig>(Configuration.GetSection("PostgreSQL"));

            //Singleton
            services.AddSingleton<ConnectionStringProvider>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
           // {
                app.UseDeveloperExceptionPage();
           // }

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "inventart_api v1");
                c.RoutePrefix = "swagger";
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = AddCorsHeader
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void AddCorsHeader(StaticFileResponseContext obj)
        {
            StringValues origins = obj.Context.Request.Headers["Origin"];
            string origin = origins.ToString();
            if (AllowedOrigins.Contains(origin))
            {
                obj.Context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            }
        }

    }
}

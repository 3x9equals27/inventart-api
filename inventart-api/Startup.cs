using Inventart.Config;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            //get the configs
            var swag = Configuration.GetSection("SwaggerConfig").Get<SwaggerConfig>();

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

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "inventart-api",
                    Version = "v1",
                    Description = "inventart-api description"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            //injectable configuration
            services.Configure<GlobalConfig>(Configuration.GetSection("GlobalConfig"));
            services.Configure<PostgresConfig>(Configuration.GetSection("PostgresConfig"));
            services.Configure<JwtConfig>(Configuration.GetSection("JwtConfig"));
            services.Configure<SmtpConfig>(Configuration.GetSection("SmtpConfig"));

            //Singleton
            services.AddSingleton<ConnectionStringProvider>();
            services.AddSingleton<JwtService>();
            services.AddSingleton<EmailService>();
            services.AddSingleton<AuthRepo>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            // {
            app.UseDeveloperExceptionPage();
            // }

            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            var swag = Configuration.GetSection("SwaggerConfig").Get<SwaggerConfig>();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.SwaggerEndpoint(swag.EndpointUrl, swag.EndpointName);
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = AddCorsHeader
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

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
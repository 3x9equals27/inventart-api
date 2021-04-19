using Inventart.Config;
using Inventart.Services.Singleton;
using Inventart.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Inventart.Repos;

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
            var auth0 = Configuration.GetSection("Auth0").Get<OAuthConfig>();

            // 1. Add Authentication Services
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = auth0.Domain;
                options.Audience = auth0.Audience;
            });

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
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows()
                    {
                        Implicit = new OpenApiOAuthFlow()
                        {
                            TokenUrl = new Uri(swag.ImplicitFlowTokenUrl),
                            AuthorizationUrl = new Uri($"{swag.ImplicitFlowAuthorizationUrl}?audience={swag.OAuthAudience}"),
                            Scopes = { }
                        }
                    }
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
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new List<string>()
                    }
                });
            });

            //AddAuthorization
            services.AddAuthorization(options =>
            {
                foreach(string permission in Permission.PermissionList)
                {
                    options.AddPolicy(permission, policy => policy.Requirements.Add(new HasScopeRequirement(permission, auth0.Domain, auth0.Namespace)));
                }
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
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
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
                c.OAuthClientId(swag.OAuthClientId);
                c.OAuth2RedirectUrl(swag.OAuth2RedirectUrl);
                c.OAuthScopeSeparator(swag.OAuthScopeSeparator);
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = AddCorsHeader
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
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

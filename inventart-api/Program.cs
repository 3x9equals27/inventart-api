using Inventart.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Inventart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string confFile = (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")??string.Empty).Equals("Development") ? "appsettings.Development.json" : "appsettings.json";

            var config = new ConfigurationBuilder()
                .AddJsonFile(confFile, optional: false)
                .Build();

            var cert = config.GetSection("SslCertConfig").Get<SslCertConfig>();

            CreateHostBuilder(args, cert).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, SslCertConfig cert) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(opts =>
                    {
                        opts.ListenAnyIP(5000);
                        opts.ListenAnyIP(5001, opts => opts.UseHttps(cert.Filename, cert.Password));
                    });
                });
    }
}
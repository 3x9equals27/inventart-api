using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Inventart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseKestrel(opts =>
                    {
                        // Bind directly to a socket handle or Unix socket
                        //opts.ListenHandle(123554);
                        //opts.ListenUnixSocket("/tmp/kestrel-test.sock");
                        opts.ListenAnyIP(5000);
                        opts.ListenAnyIP(5001, opts => opts.UseHttps());
                    });
                });
    }
}
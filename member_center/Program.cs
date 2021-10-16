using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog.Web;
using Winton.Extensions.Configuration.Consul;

namespace member_center
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
                    webBuilder.ConfigureAppConfiguration((ctx,cfg)=> {
                        var localconfig = new ConfigurationBuilder()
                                    .SetBasePath(Directory.GetCurrentDirectory())
                                    .AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();
                    
                    });
               
                    webBuilder.UseStartup<Startup>();
                });
    }
}

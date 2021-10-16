using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ordering.services;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ordering
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext().MinimumLevel.Information()
             .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch-client:9200/"))
             {
                 AutoRegisterTemplate = true,
                 AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                 IndexFormat = "abp-order-log-{0:yyyy.MM}",
                 FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                 EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.WriteToFailureSink |
                                   EmitEventFailureHandling.RaiseCallback,
                 FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)
             }).WriteTo.Console()
             .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {



            services.AddSingleton<IMemberService, MemberService>();
            services.AddControllers();
       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseAllElasticApm(Configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

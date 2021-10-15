
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;
using Serilog.Formatting.Json;

namespace hotel_base
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment env)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                 .Enrich.FromLogContext().MinimumLevel.Information()
                 .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://elasticsearch-client:9200/"))
                 {
                     AutoRegisterTemplate = true,
                     AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                     IndexFormat = "abp-demo-log-{0:yyyy.MM}",
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
          

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();
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

using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using common.libs;
using Consul;
using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ordering.services;
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
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //ע��Consulclient����
            services.AddSingleton<IConsulClient>(new ConsulClient(x => {
                x.Address = new Uri(Configuration["consul:clientAddress"]);
            }));
            //ע��ConsulService�����װ��һЩ����
            services.AddSingleton<IConsulService, ConsulService>();
            services.AddSingleton<IMemberService, MemberService>();

            //ע��ConsulRegisterService ���servcie��app������ʱ����Զ�ע�������Ϣ
            services.AddHostedService<ConsulRegisterService>();


            services.AddControllers();
            services.ConfigureDynamicProxy();
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

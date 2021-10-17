
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
using System.IO;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ.Client;

namespace hotel_base
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                 .Enrich.FromLogContext().MinimumLevel.Information()
                 .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://192.168.1.3:30200/"))
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

            var config = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json")
.Build();

            Log.Information($"Received");

            var factory = new ConnectionFactory();
            factory.HostName = "rabbitmq-cluster";//��������Rabbit�������IP����һ��endpoint���������Ϥ�ɣ�����socket�󶨵��Ǹ��ս�㡣
            factory.UserName = "admin";//Ĭ���û���,�û������ڷ�����Զ��崴���������������
            factory.Password = "admin";//Ĭ������
            factory.Port = 5672;
            using (var connection = factory.CreateConnection())//���ӷ������������ڴ����ս�㡣
            {
                //����һ��ͨ�����������Rabbit�Լ�����Ĺ����ˣ�����Լ�д��Ϣ���У�����Ϳ��Կ��Զ������
                //����Rabbit���淨����һ��ͨ��channel�°����������Queue
                using (var channel = connection.CreateModel())
                {




                    /* ���ﶨ����һ�������ߣ��������ѷ��������ܵ���Ϣ
                     * C#������Ҫע���������һЩ�����������������Ƚϲ�������У��Ƿǳ������������ģʽ�ġ�
                     * ����RabbitMQʹ������������������ģʽ��Ȼ��ܶ���ص�ʹ�����¶�������������ߺ���������������
                     * ���ǣ���C#��������������߶����Ƕ��ԣ������㲻��һ�����ģʽ��������һ��������Ĵ����д����
                     * ���ԣ���Ҳ�Ҫ���ӵ������ŵ�����ʵ����û��ô���ӡ�
                     * �����ʵ���Ƕ���һ��EventingBasicConsumer���͵Ķ���Ȼ��ö����и�Received�¼���
                     * ���¼����ڷ�����յ�����ʱ������
                     */

                    channel.QueueDeclare("redis", false, false, false, null);
                    var consumer = new EventingBasicConsumer(channel);//������
                    channel.BasicQos(0, 1, false);
                    channel.BasicConsume("redis", false, consumer);//������Ϣ
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());

                        Log.Information($"Received�� {message}");

                        Log.Information($"do�� {message}");
                        //������ɣ�����Broker���Է���˿���ɾ����Ϣ�������µ���Ϣ����
                        channel.BasicAck(ea.DeliveryTag, false);
                    };





                }
            }


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

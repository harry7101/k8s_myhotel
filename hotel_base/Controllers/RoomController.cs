using hotel_base.models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Sinks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace hotel_base.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {
        private static List<RoomVM> _rooms = new List<RoomVM>() {
            new RoomVM{
                Floor =1,
                Id= "11",
                No = "1001",
                HotelId = "H8001"
            },
            new RoomVM{
             Floor =1,
                Id= "12",
                No = "1002",
                HotelId = "H8001"
            },
            new RoomVM{
             Floor =2,
                Id= "21",
                No = "2001",
                HotelId = "H8001"
            },
            new RoomVM{
             Floor =2,
                Id= "22",
                No = "2002",
                HotelId = "H8002"
            },
              new RoomVM{
             Floor =3,
                Id= "31",
                No = "3001",
                HotelId = "H8002"
            },
            new RoomVM{
             Floor =3,
                Id= "32",
                No = "3002",
                HotelId = "H8002"
            },
        };

        private readonly ILogger<RoomController> _logger;
        private readonly IConfiguration _configuration;
        public RoomController(ILogger<RoomController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<RoomVM> Get()
        {
            _logger.LogInformation("gettime:"+DateTime.Now.ToString());
            return _rooms;
        }

        [HttpGet("{no}")]
        public RoomVM Get(string no)
        {
            return _rooms.FirstOrDefault(x=>x.No == no);
        }

        [HttpGet("hotel_rooms/{hotelId}")]
        public List<RoomVM> HotelRooms(string hotelId)
        {
            return _rooms.Where(x => x.HotelId == hotelId).ToList();
        }


        [HttpGet("getroom")]
        public string GetRoomId(string Id)
        {
            try
            {
                RoomVM entity = new RoomVM() { Id = Id, HotelId = Id };
                var cstring = _configuration["Redis:Configuration"];

                using (var cache = ConnectionMultiplexer.Connect(cstring))
                {
                    IDatabase db = cache.GetDatabase();
                    bool setValue = db.HashSet("addroom", entity.Id, entity.HotelId);

                }

                var factory = new ConnectionFactory();
                factory.HostName = "rabbitmq-cluster";//主机名，Rabbit会拿这个IP生成一个endpoint，这个很熟悉吧，就是socket绑定的那个终结点。
                factory.UserName = "admin";//默认用户名,用户可以在服务端自定义创建，有相关命令行
                factory.Password = "admin";//默认密码
                factory.Port = 5672;
                using (var connection = factory.CreateConnection())//连接服务器，即正在创建终结点。
                {
                    //创建一个通道，这个就是Rabbit自己定义的规则了，如果自己写消息队列，这个就可以开脑洞设计了
                    //这里Rabbit的玩法就是一个通道channel下包含多个队列Queue
                    using (var channel = connection.CreateModel())
                    {






                        channel.ExchangeDeclare("exchangeName", ExchangeType.Direct);
                        channel.QueueDeclare("redis", false, false, false, null);
                        channel.QueueBind("redis", "exchangeName", "addroom", null);
                        var properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 1;
                        string message = $"Id:{entity.Id}"; //传递的消息内容
                        channel.BasicPublish("exchangeName", "addroom", properties, Encoding.UTF8.GetBytes(message));



                    }
                }

                return Id;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"getroom:{ex.Message}");
                return ex.Message;
            }
        }

    }
}

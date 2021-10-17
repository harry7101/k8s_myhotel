﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace receviceRabbitMq
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";//主机名，Rabbit会拿这个IP生成一个endpoint，这个很熟悉吧，就是socket绑定的那个终结点。
            factory.UserName = "admin";//默认用户名,用户可以在服务端自定义创建，有相关命令行
            factory.Password = "admin";//默认密码
            factory.Port = 30001;
            using (var connection = factory.CreateConnection())//连接服务器，即正在创建终结点。
            {
                //创建一个通道，这个就是Rabbit自己定义的规则了，如果自己写消息队列，这个就可以开脑洞设计了
                //这里Rabbit的玩法就是一个通道channel下包含多个队列Queue
                using (var channel = connection.CreateModel())
                {




                    /* 这里定义了一个消费者，用于消费服务器接受的消息
                     * C#开发需要注意下这里，在一些非面向对象和面向对象比较差的语言中，是非常重视这种设计模式的。
                     * 比如RabbitMQ使用了生产者与消费者模式，然后很多相关的使用文章都在拿这个生产者和消费者来表述。
                     * 但是，在C#里，生产者与消费者对我们而言，根本算不上一种设计模式，他就是一种最基础的代码编写规则。
                     * 所以，大家不要复杂的名词吓到，其实，并没那么复杂。
                     * 这里，其实就是定义一个EventingBasicConsumer类型的对象，然后该对象有个Received事件，
                     * 该事件会在服务接收到数据时触发。
                     */

                    channel.QueueDeclare("redis", false, false, false, null);
                    var consumer = new EventingBasicConsumer(channel);//消费者
                    channel.BasicQos(0, 1, false);
                    channel.BasicConsume("redis", false, consumer);//消费消息
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());

                        Console.WriteLine("Received： {0}", message);
                        Console.WriteLine("Received： {0}", ea.ConsumerTag);

                        Console.WriteLine("do： {0}", message);
                        //处理完成，告诉Broker可以服务端可以删除消息，分配新的消息过来
                        channel.BasicAck(ea.DeliveryTag, false);
                    };





                }
            }
        }
    }
}

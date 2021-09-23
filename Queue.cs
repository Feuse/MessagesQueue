using Newtonsoft.Json;
using RabbitMQ.Client;
using ServicesInterfaces.Scheduler;
using ServicesModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagesQueue
{
    public class Queue : IQueue
    {
        public void QueueMessage(Message message)
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672
            };
            try
            {

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "messages",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
                        var body = SerializeJson(message);
                        channel.BasicPublish(exchange: "",
                                             routingKey: "messages",
                                             basicProperties: null,
                                             body: body);
                    }
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }
        public byte[] SerializeJson(Message message)
        {
            var newMessage = JsonConvert.SerializeObject(message);
            var encoded = Encoding.UTF8.GetBytes(newMessage);
            return encoded;
        }


    }
}

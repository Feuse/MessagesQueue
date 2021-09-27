using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ServicesInterfaces.Global;
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
        private readonly IAppSettings appSettings;
        private IList<AmqpTcpEndpoint> endpoints;
        public Queue(IAppSettings _appSettings)
        {
            appSettings = _appSettings;
            InitAmqp();
        }
        public void InitAmqp()
        { 
            endpoints = new List<AmqpTcpEndpoint>();
            foreach (var port in appSettings.QueuePorts)
            {
                endpoints.Add(new AmqpTcpEndpoint(appSettings.HostName, port));
            }
        }
        public void QueueMessage(Message message)
        {
            ConnectionFactory factory = new ConnectionFactory();
            try
            {
                using (var connection = factory.CreateConnection(endpoints))
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: appSettings.Queue,
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
                        var body = SerializeJson(message);
                        channel.BasicPublish(exchange: "",
                                             routingKey: appSettings.Queue,
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

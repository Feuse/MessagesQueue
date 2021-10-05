using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<Queue> _logger;
        private readonly IAppSettings appSettings;
        private IList<AmqpTcpEndpoint> endpoints;
        public Queue(IAppSettings _appSettings, ILogger<Queue> logger)
        {
            appSettings = _appSettings;
            _logger = logger;
            //InitAmqp();
        }
        public void InitAmqp()
        {
            try
            {
                endpoints = new List<AmqpTcpEndpoint>();
                foreach (var port in appSettings.QueuePorts)
                {
                    endpoints.Add(new AmqpTcpEndpoint(appSettings.HostName, port));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);

                throw;
            }
        }
        public void QueueMessage(Message message)
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory() { Uri = new Uri(appSettings.AMQP_URL) };

                using (var connection = factory.CreateConnection())
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
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                throw;
            }
        }
        public byte[] SerializeJson(Message message)
        {
            try
            {
                var newMessage = JsonConvert.SerializeObject(message);
                var encoded = Encoding.UTF8.GetBytes(newMessage);
                return encoded;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                _logger.LogTrace(e.StackTrace);
                throw;
            }
        }
    }
}

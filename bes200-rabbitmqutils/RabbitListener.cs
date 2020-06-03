using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqUtils
{
    /* "UserName": "guest",
    "Password": "guest",
    "HostName": "localhost",
    "VHost": "/",
    "Port": 5672*/

    public abstract class RabbitListener : BackgroundService
    {

        private readonly IConnection Connection;
        private readonly IModel Channel;
        private readonly RabbitOptions Options;
        public RabbitListener(IOptionsMonitor<RabbitOptions> options)
        {
            var settings = options.CurrentValue;

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = settings.HostName,
                    UserName = settings.UserName,
                    Password = settings.Password,
                    Port = settings.Port,
                    VirtualHost = settings.VHost
                };
                this.Connection = factory.CreateConnection();
                this.Channel = Connection.CreateModel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Initialization failed: " + ex.Message);
                throw ex;
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Register();
            return Task.CompletedTask;
        }

        public abstract Task<bool> Process(string message);

        protected string RouteKey;
        protected string QueueName;
        protected string ExchangeName;
        public async void Register()
        {
            if (ExchangeName != "")
            {
                Channel.ExchangeDeclare(exchange: ExchangeName, type: "direct", durable: true);
            }
            Channel.QueueDeclare(queue: QueueName, exclusive: false, durable: true, autoDelete: false);
            if (ExchangeName != "")
            {
                Channel.QueueBind(queue: QueueName,
                              exchange: ExchangeName,
                              routingKey: RouteKey);
            }
            var consumer = new EventingBasicConsumer(Channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                var result = Process(message).Result;
                if (result)
                {
                    Channel.BasicAck(ea.DeliveryTag, false);
                }
            };
            Channel.BasicConsume(queue: QueueName, consumer: consumer);
        }

        public void DeRegister()
        {
            this.Connection.Close();
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.Connection.Close();
            return Task.CompletedTask;
        }
    }
}

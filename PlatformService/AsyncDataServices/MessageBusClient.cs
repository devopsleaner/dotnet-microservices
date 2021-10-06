using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Platformservice.Dtos;
using RabbitMQ.Client;

namespace Platformservice.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private const string Exchange = "trigger";
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"] ,
                Port =  int.Parse(_configuration["RabbitMQPort"])
            };  

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: Exchange, type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                System.Console.WriteLine("---> Connected to Message bus");
            }
            catch(Exception ex)
            {   
                System.Console.WriteLine("--->Error connecting Message Bus. ex="+ex.Message );
            }

        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("--->Rabbit mq connection is shutdown");
        }

        public void PublishNewPlatform(PlatformPublishDto platformPublishDto)
        {

            var message = JsonSerializer.Serialize(platformPublishDto);

            if(_connection.IsOpen)
            {
                System.Console.WriteLine("---> Rabbit MQ Connnection is open, sending message");
                SendMessage(message);
            }
            else
            {
                System.Console.WriteLine("---> RabbitMQ connection is not open. not sending");
            }
        }

        private void SendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange:Exchange, routingKey:"", basicProperties:null, body:body);

            System.Console.WriteLine($"===> we have successfully sent message to mq - {message}");
        }


        public void Dispose(){
            System.Console.WriteLine("Message bus disposed");

            if(_channel.IsOpen){

                _channel.Close();
                _connection.Close();
            }
        }
    }
}
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventsProcessor _eventsProcessor;
        private IConnection _connection;
        private IModel _channel;
        private string _queueName;
        private const string Exchange = "trigger";


        public MessageBusSubscriber(IConfiguration configuration, IEventsProcessor eventsProcessor)
        {
            _configuration = configuration;

            _eventsProcessor = eventsProcessor;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
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
                _queueName = _channel.QueueDeclare().QueueName;
                _channel.QueueBind(queue: _queueName,
                                    exchange: Exchange,
                                    routingKey: "" );

                System.Console.WriteLine("---> listening on the Message bus from command service!");

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            }
            catch(Exception ex)
            {   
                System.Console.WriteLine("--->Command service - Error connecting Message Bus. ex="+ex.Message );
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
               System.Console.WriteLine("--->Rabbit mq connection is shutdown - command service");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
           stoppingToken.ThrowIfCancellationRequested();

           var consumer = new EventingBasicConsumer(_channel);

           consumer.Received+= (ModuleHandle, ea)=>
           {
               System.Console.WriteLine("---> Event Received!!");
               
               var body = ea.Body;

               var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                _eventsProcessor.ProcessEvents(notificationMessage);
           };

           _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

           return Task.CompletedTask;
        }

        public override void Dispose()
        {
            if(_channel.IsOpen){
                _channel.Close();
                _connection.Close();
            }
            base.Dispose();
        }
    }
}
using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventsProcessor : IEventsProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IMapper _mapper;

        public EventsProcessor(IServiceScopeFactory scopeFactory,IMapper mapper)
        {
            _scopeFactory = scopeFactory;
            _mapper = mapper;

        }
        public void ProcessEvents(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    addPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType DetermineEvent(string notificationMessage){
            System.Console.WriteLine("---> Determining Event");
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch(eventType.Event)
            {
                case "Platform_Published":
                    System.Console.WriteLine("--->Platform Publish event detected");
                    return EventType.PlatformPublished;
                default:
                    System.Console.WriteLine("--->Could not determine the event tyoe");
                    return EventType.Undetermined;
            }
        }

        private void addPlatform(string platformPublishedMessage)
        {
              System.Console.WriteLine("---> About to add platform");
            using(var scope = _scopeFactory.CreateScope()){
                var repo = scope.ServiceProvider.GetRequiredService<ICommandsRepo>();

                var platformPublishedDTO = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                try{
                    var platform = _mapper.Map<Platform>(platformPublishedDTO);

                    if(!repo.ExternamPlatformExists(platform.ExternalId)){
                        repo.CreatePlatform(platform);
                        repo.SaveChanges();
                          System.Console.WriteLine("---> Platform added!!!");
                    }
                    else{
                        System.Console.WriteLine("---> Platform already exists");
                    }
                }
                catch(Exception ex){
                    System.Console.WriteLine($"---> Could not add platform to db - {ex.Message}");
                }
            }
        }
    }
}
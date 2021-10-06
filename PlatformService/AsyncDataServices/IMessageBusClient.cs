using Platformservice.Dtos;

namespace Platformservice.AsyncDataServices
{
    public interface  IMessageBusClient
    {
        void PublishNewPlatform(PlatformPublishDto platformPublishDto);
    }
}
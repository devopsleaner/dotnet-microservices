using System.Threading.Tasks;
using Platformservice.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public interface ICommandDataClient
    {
         Task SendPlatformToCommand(PlatformReadDto platform);
    }    
}
using System.Collections.Generic;
using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.Data
{
    public static class PrepDb
    {

        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using(var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();
                var commandRepo = serviceScope.ServiceProvider.GetService<ICommandsRepo>();

                var platforms = grpcClient.ReturnAllPlatforms();
                
                SeedData(commandRepo, platforms);
            }
        }

        private static void SeedData(ICommandsRepo repo , IEnumerable<Platform> platforms)
        {
            System.Console.WriteLine("===> Seeding new platforms from platforms service.....");


            foreach(var platform in platforms)
            {
                if(!repo.ExternamPlatformExists(platform.ExternalId)){
                    repo.CreatePlatform(platform);
                    repo.SaveChanges();
                    System.Console.WriteLine($"Platform seeded - platform external id = {platform.ExternalId}");

                }
            }
        }
    }
}
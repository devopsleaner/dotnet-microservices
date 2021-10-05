using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Platformservice.Models;

namespace Platformservice.Data
{
    public static class PrepDb    
    {
        public static void PrepPopulation(IApplicationBuilder app,bool isProduction)
        {
            using(var serviceScope = app.ApplicationServices.CreateScope())
            {
                SeedData(serviceScope.ServiceProvider.GetService<AppDbContext>(),isProduction);
            }  
        }

        private static void SeedData(AppDbContext context, bool isProduction)
        {
            if(isProduction)
            {
                System.Console.WriteLine("---> Attempting to apply migrations");
                try{
                    context.Database.Migrate();
                }
                catch(Exception ex)
                {
                    System.Console.WriteLine("Cannot apply migrations. exception = "+ ex.Message);
                }
            }
            if(!context.Platforms.Any())
            {
                Console.WriteLine("---> Seeding Data Start.....");

                context.Platforms.AddRange(

                    new Platform(){Name="Dot Net", Publisher= "Microsoft", Cost="Free"},
                    new Platform(){Name="SQL Server Express", Publisher= "Microsoft", Cost="Free"},
                    new Platform(){Name="Kubernetes", Publisher= "Cloud Native Foundation", Cost="Free"},
                    new Platform(){Name="Docker", Publisher= "Docker Inc", Cost="Free"}

                );

                context.SaveChanges();

                Console.WriteLine("---> Seeding Data End.....");
            }
            else
            {
                Console.WriteLine("---> We already have data");
            }
        }

    }
}
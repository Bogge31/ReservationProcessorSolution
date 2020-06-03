using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMqUtils;

namespace ReservationProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var rabbitConfig = hostContext.Configuration.GetSection("rabbit");
                    services.Configure<RabbitOptions>(rabbitConfig);
                    services.AddHttpClient<ReserverationHttpService>();
                    services.AddHostedService<ReservationListener>();
                });
    }
}

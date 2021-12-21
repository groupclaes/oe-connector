using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GroupClaes.OpenEdge.Connector
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(x => {
              x.ServiceName = "GroupClaes OpenEdge Connector";
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>();
            });
  }
}

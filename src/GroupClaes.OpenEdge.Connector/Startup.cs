using GroupClaes.OpenEdge.Connector.Business.Extensions;
using GroupClaes.OpenEdge.Connector.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace GroupClaes.OpenEdge.Connector
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddStackExchangeRedisCache(options =>
        options.Configuration = Configuration["Redis:ConnectionString"]);

      bool redisEnabled = Configuration.GetValue("Redis:Enabled", false);
      services.AddOpenEdge(redisEnabled);

      services.AddControllers();
      services.AddLogging(opt =>
      {
        opt.AddConsole();
      });

      services.AddSignalR()
        .AddMessagePackProtocol();

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "GroupClaes.OpenEdge.Connector", Version = "v1" });
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (true && env.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GroupClaes.OpenEdge.Connector v1"));
      }

      //app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

#if DEBUG 
      app.UseRequestDebugging();
#endif

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<OpenEdgeHub>("/openedge");
      });
    }
  }
}

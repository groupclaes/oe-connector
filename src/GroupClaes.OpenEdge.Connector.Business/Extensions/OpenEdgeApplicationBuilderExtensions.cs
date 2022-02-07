using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace GroupClaes.OpenEdge.Connector.Business.Extensions
{
  public static class OpenEdgeApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseRequestDebugging(this IApplicationBuilder app)
    {
      return app.Use(async (x, next) =>
      {
        MemoryStream ms = new MemoryStream();
        await x.Request.Body.CopyToAsync(ms);
        StreamReader sr = new StreamReader(ms);
        ms.Seek(0, SeekOrigin.Begin);

        string debug = await sr.ReadToEndAsync();
        var logger = app.ApplicationServices.GetRequiredService<ILogger>();
        logger.LogError("{Content} {ContentLength} {RequestLength}", debug, debug.Length, x.Request.ContentLength);
        ms.Seek(0, SeekOrigin.Begin);

        x.Request.Body = ms;

        await next.Invoke();
      });
    }
  }
}

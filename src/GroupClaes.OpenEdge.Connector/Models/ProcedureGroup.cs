using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GroupClaes.OpenEdge.Connector.Models
{
  public class ProcedureGroup
  {
    public string Procedure { get; init; }
    public Stopwatch Stopwatch { get; init; }

    public IReadOnlyCollection<string> Recipients
    {
      get
      {
        semaphore.Wait();
        string[] result = recipients.ToArray();
        semaphore.Release();

        return result;
      }
    }

    private List<string> recipients;
    private SemaphoreSlim semaphore;

    public ProcedureGroup(string procedureName)
    {
      Stopwatch = Stopwatch.StartNew();
      Procedure = procedureName;

      recipients = new List<string>();
      semaphore = new SemaphoreSlim(1, 1);
    }

    public void AddRecipient(string recipient)
    {
      semaphore.Wait();
      recipients.Add(recipient);
      semaphore.Release();
    }

    public void Initialize()
    {
      semaphore.Wait();

      if (!Stopwatch.IsRunning)
      {
        Stopwatch.Restart();
      }

      semaphore.Release();
    }

    public void Complete()
    {
      semaphore.Wait();

      recipients.Clear();
      Stopwatch.Stop();

      semaphore.Release();
    }
  }
}
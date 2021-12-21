using System.Text.RegularExpressions;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public class Regexes
  {
    public static readonly Regex HTTPStatusCode = new Regex(@"^[1-9][0-9]{2}$");
  }
}

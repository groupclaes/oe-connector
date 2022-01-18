using System.Text.RegularExpressions;

namespace GroupClaes.OpenEdge.Connector.Shared
{
  public static class Regexes
  {
    public const string HttpStatusCodeString = @"^[1-9][0-9]{2}$";
    public const string ProcedurePathString = @"^([\w-./]{1,223}\/)?([\w-.]{1,32})$";
    public const string AppServerString = @"^[\w-]+$";
    public const string UsernameString = @"^[\w-]+$";
    public const string PasswordString = @"^[\w-@$!%*#?&]+$";
    public const string LabelString = @"^[a-z][a-zA-Z0-9-]*$";

    public static readonly Regex HttpStatusCode = new Regex(HttpStatusCodeString);

    public static readonly Regex ProcedurePath = new Regex(ProcedurePathString);
    public static readonly Regex Label = new Regex(LabelString);
  }
}

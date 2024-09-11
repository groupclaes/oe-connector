namespace GroupClaes.OpenEdge.Connector.Business.Raw
{
  public class AppServerConfig
  {
    private string _endpoint;

    public string Endpoint
    {
      get => _endpoint;
      set => SetConnectionString(value);
    }
    public string AppId { get; set; }
    public string Username { get; set; }
    public string Password { get; set;  }
    public string PathPrefix { get; set; }

    internal void SetConnectionString(string connectionString)
    {
      if (!connectionString.StartsWith("http://"))
        _endpoint = "http://" + connectionString;
      else
        _endpoint = connectionString;
    }
  }
}

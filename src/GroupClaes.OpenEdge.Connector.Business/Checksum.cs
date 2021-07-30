using System;
using System.Security.Cryptography;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public class Checksum
  {
    public static string Generate(StringBuilder stringBuilder)
    {
      using (var sha = SHA256.Create())
      {
        byte[] checksumData = sha.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
        return BitConverter.ToString(checksumData).Replace("-", String.Empty);
      }
    }
  }
}
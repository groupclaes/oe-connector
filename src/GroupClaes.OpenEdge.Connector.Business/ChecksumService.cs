using System;
using System.Security.Cryptography;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Business
{
  internal class ChecksumService : IChecksumService
  {
    public string Generate(string value)
    {
      using (SHA256 sha = SHA256.Create())
      {
        byte[] checksumData = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return BitConverter.ToString(checksumData)
          .Replace("-", string.Empty);
      }
    }

    public string Generate(StringBuilder stringBuilder)
      => Generate(stringBuilder.ToString());
  }
}

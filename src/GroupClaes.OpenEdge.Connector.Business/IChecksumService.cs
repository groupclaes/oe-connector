using System;
using System.Collections.Generic;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Business
{
  public interface IChecksumService
  {
    string Generate(string value);
    string Generate(StringBuilder stringBuilder);
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public enum ParameterType : int
  {
    String = 1,
    Date = 2,
    Boolean = 3,
    Byte = 8,
    LongChar = 39,

    /* Numbers  */
    Integer = 4,
    Decimal = 5,
    Long = 7,
    Int64 = 41,

    Handle = 10,
    MemPointer = 11,
    RowId = 13,
    COMHandle = 14,
    DataTable = 15,
    DynDataTable = 17,
    DataSet = 36,
    DynDataSet = 37,
    DateTime = 34,
    DateTimeTZ = 40,
  }
}

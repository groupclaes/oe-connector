namespace GroupClaes.OpenEdge.Connector.Shared.Models
{
  public enum ParameterType : int
  {
    Undefined = 0,
    String = 1,
    Date = 2,
    Boolean = 3,
    Byte = 8,
    LongChar = 39,

    /* Numbers  */
    Integer = 4,
    Decimal = 5,
    Long = 7,
    Integer64 = 41,

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

    /* Aliasses */
    Bool = Boolean,
    Int = Integer,
    Int64 = Integer64,


    /* Custom types to be parsed internally */
    JSON = 15001,
    File = MemPointer
  }
}

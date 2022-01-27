namespace LBPUnion.ProjectLighthouse.Types.Tickets;

public enum DataType : byte
{
    Empty = 0x00,
    UInt32 = 0x01,
    UInt64 = 0x02,
    String = 0x04,
    Timestamp = 0x07,
    Binary = 0x08,
}
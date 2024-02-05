using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LBPUnion.ProjectLighthouse.Tickets;

public abstract class TicketData
{
    protected void WriteHeader(BinaryWriter writer)
    {
        byte[] id = new byte[2];
        byte[] len = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(id, (ushort)this.Id());
        BinaryPrimitives.WriteUInt16BigEndian(len, (ushort)this.Len());
        writer.Write(id);
        writer.Write(len);
    }

    public abstract void Write(BinaryWriter writer);
    protected abstract short Id();
    public abstract short Len();
}

#region Empty Data
public class EmptyData : TicketData
{
    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
    }

    protected override short Id() => 0;
    public override short Len() => 0;
}
#endregion

#region UInt32 Data
public class UInt32Data : TicketData
{
    private readonly uint val;

    public UInt32Data(uint val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        byte[] data = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(data, this.val);
        writer.Write(data);
    }

    protected override short Id() => 1;
    public override short Len() => 4;
}
#endregion

#region UInt64 Data
public class UInt64Data : TicketData
{
    private readonly ulong val;

    public UInt64Data(ulong val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        byte[] data = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(data, this.val);
        writer.Write(data);
    }

    protected override short Id() => 2;
    public override short Len() => 8;
}
#endregion

#region String Data
public class StringData : TicketData
{
    private readonly byte[] val;

    public StringData(string val)
    {
        this.val = Encoding.ASCII.GetBytes(val);
    }

    public StringData(byte[] val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        writer.Write(this.val);
    }

    protected override short Id() => 4;
    public override short Len() => (short)this.val.Length;
}
#endregion

#region Timestamp Data
public class TimestampData : TicketData
{
    private readonly ulong val;

    public TimestampData(ulong val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        byte[] data = new byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(data, this.val);
        writer.Write(data);
    }

    protected override short Id() => 7;
    public override short Len() => 8;
}
#endregion

#region Binary Data
public class BinaryData : TicketData
{
    private readonly byte[] val;

    public BinaryData(byte[] val)
    {
        this.val = val;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        writer.Write(this.val);
    }

    protected override short Id() => 8;
    public override short Len() => (short)this.val.Length;
}
#endregion

#region Blob (Section) Data
public class BlobData : TicketData
{
    private readonly byte id;
    private readonly List<TicketData> data;

    public BlobData(byte id, List<TicketData> data)
    {
        this.id = id;
        this.data = data;
    }

    public override void Write(BinaryWriter writer)
    {
        this.WriteHeader(writer);
        foreach (TicketData d in this.data)
        {
            d.Write(writer);
        }
    }

    protected override short Id() => (short)(0x3000 | this.id);
    public override short Len() => (short)this.data.Sum(d => d.Len() + 4);
}
#endregion
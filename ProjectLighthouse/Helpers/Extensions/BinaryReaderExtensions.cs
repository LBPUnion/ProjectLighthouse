using System;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

public static class BinaryReaderExtensions
{

    #region Big Endian reading

    // Yoinked from https://stackoverflow.com/questions/8620885/c-sharp-binary-reader-in-big-endian
    public static byte[] Reverse(this byte[] b)
    {
        Array.Reverse(b);
        return b;
    }

    public static ushort ReadUInt16BE(this BinaryReader binRdr) => BitConverter.ToUInt16(binRdr.ReadBytesRequired(sizeof(ushort)).Reverse(), 0);

    public static short ReadInt16BE(this BinaryReader binRdr) => BitConverter.ToInt16(binRdr.ReadBytesRequired(sizeof(short)).Reverse(), 0);

    public static uint ReadUInt32BE(this BinaryReader binRdr) => BitConverter.ToUInt32(binRdr.ReadBytesRequired(sizeof(uint)).Reverse(), 0);

    public static int ReadInt32BE(this BinaryReader binRdr) => BitConverter.ToInt32(binRdr.ReadBytesRequired(sizeof(int)).Reverse(), 0);

    public static ulong ReadUInt64BE(this BinaryReader binRdr) => BitConverter.ToUInt32(binRdr.ReadBytesRequired(sizeof(ulong)).Reverse(), 0);

    public static long ReadInt64BE(this BinaryReader binRdr) => BitConverter.ToInt32(binRdr.ReadBytesRequired(sizeof(long)).Reverse(), 0);

    public static byte[] ReadBytesRequired(this BinaryReader binRdr, int byteCount)
    {
        byte[] result = binRdr.ReadBytes(byteCount);

        if (result.Length != byteCount) throw new EndOfStreamException($"{byteCount} bytes required from stream, but only {result.Length} returned.");

        return result;
    }

    #endregion

}
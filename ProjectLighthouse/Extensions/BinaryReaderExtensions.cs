using System;
using System.Buffers.Binary;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class BinaryReaderExtensions
{

    #region Big Endian reading

    // Yoinked from https://stackoverflow.com/questions/8620885/c-sharp-binary-reader-in-big-endian
    public static byte[] Reverse(this byte[] b)
    {
        Array.Reverse(b);
        return b;
    }

    public static ushort ReadUInt16BE(this BinaryReader binRdr) => BinaryPrimitives.ReadUInt16BigEndian(binRdr.ReadBytesRequired(sizeof(ushort)));

    public static short ReadInt16BE(this BinaryReader binRdr) => BinaryPrimitives.ReadInt16BigEndian(binRdr.ReadBytesRequired(sizeof(short)));

    public static uint ReadUInt32BE(this BinaryReader binRdr) => BinaryPrimitives.ReadUInt32BigEndian(binRdr.ReadBytesRequired(sizeof(uint)));

    public static int ReadInt32BE(this BinaryReader binRdr) => BinaryPrimitives.ReadInt32BigEndian(binRdr.ReadBytesRequired(sizeof(int)));

    public static ulong ReadUInt64BE(this BinaryReader binRdr) => BinaryPrimitives.ReadUInt64BigEndian(binRdr.ReadBytesRequired(sizeof(ulong)));

    public static long ReadInt64BE(this BinaryReader binRdr) => BinaryPrimitives.ReadInt64BigEndian(binRdr.ReadBytesRequired(sizeof(long)));

    private static byte[] ReadBytesRequired(this BinaryReader binRdr, int byteCount)
    {
        byte[] result = binRdr.ReadBytes(byteCount);

        if (result.Length != byteCount) throw new EndOfStreamException($"{byteCount} bytes required from stream, but only {result.Length} returned.");

        return result;
    }

    #endregion

}
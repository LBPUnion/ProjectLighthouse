using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class BinaryHelper
    {
        public static string ReadString(BinaryReader reader)
        {
            List<byte> readBytes = new();

            byte readByte;
            do readBytes.Add(readByte = reader.ReadByte());
            while (readByte != 0x00);

            return Encoding.UTF8.GetString(readBytes.ToArray());
        }

        public static void ReadUntilByte(BinaryReader reader, byte byteToReadTo)
        {
            byte readByte;
            do readByte = reader.ReadByte();
            while (readByte != byteToReadTo);
        }

        public static byte[] ReadLastBytes(BinaryReader reader, int count, bool restoreOldPosition = true)
        {
            long oldPosition = reader.BaseStream.Position;

            if (reader.BaseStream.Length < count) return Array.Empty<byte>();

            reader.BaseStream.Position = reader.BaseStream.Length - count;
            byte[] data = reader.ReadBytes(count);

            if (restoreOldPosition) reader.BaseStream.Position = oldPosition;
            return data;
        }

        // Written with reference from
        // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/request-response?view=aspnetcore-5.0
        // Surprisingly doesn't take seconds. (67ms for a 100kb file)
        public static async Task<byte[]> ReadFromPipeReader(PipeReader reader)
        {
            List<byte> data = new();
            while (true)
            {
                ReadResult readResult = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = readResult.Buffer;

                if (readResult.IsCompleted && buffer.Length > 0) data.AddRange(buffer.ToArray());

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted) break;
            }

            return data.ToArray();
        }
    }
}
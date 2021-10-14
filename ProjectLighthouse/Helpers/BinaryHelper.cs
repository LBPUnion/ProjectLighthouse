using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProjectLighthouse.Helpers {
    public static class BinaryHelper {
        public static string ReadString(BinaryReader reader) {
            List<byte> readBytes = new();

            byte readByte;
            do {
                readBytes.Add(readByte = reader.ReadByte());
            } while(readByte != 0x00);

            return Encoding.UTF8.GetString(readBytes.ToArray());
        }

        public static void ReadUntilByte(BinaryReader reader, byte byteToReadTo) {
            byte readByte;
            do {
                readByte = reader.ReadByte();
            } while(readByte != byteToReadTo);
        }
    }
}
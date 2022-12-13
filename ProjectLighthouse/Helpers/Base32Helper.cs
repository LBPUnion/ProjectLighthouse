using System;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class Base32Encoding
    {
        public static byte[] ToBytes(string input)
        {
            if(string.IsNullOrEmpty(input)) throw new ArgumentNullException(nameof(input));

            input = input.TrimEnd('='); // remove padding characters
            int byteCount = input.Length * 5 / 8; // this must be truncated
            byte[] returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int arrayIndex = 0;

            foreach (int cValue in input.Select(CharToValue))
            {
                int mask;
                if(bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            // if we didn't end with a full byte
            if(arrayIndex != byteCount)
            {
                returnArray[arrayIndex] = curByte;
            }

            return returnArray;
        }

        public static string ToString(byte[] input)
        {
            if(input == null || input.Length == 0)
            {
                throw new ArgumentNullException(nameof(input));
            }

            int charCount = (int)Math.Ceiling(input.Length / 5d) * 8;
            char[] returnArray = new char[charCount];

            byte nextChar = 0, bitsRemaining = 5;
            int arrayIndex = 0;

            foreach(byte b in input)
            {
                nextChar = (byte)(nextChar | (b >> (8 - bitsRemaining)));
                returnArray[arrayIndex++] = ValueToChar(nextChar);

                if(bitsRemaining < 4)
                {
                    nextChar = (byte)((b >> (3 - bitsRemaining)) & 31);
                    returnArray[arrayIndex++] = ValueToChar(nextChar);
                    bitsRemaining += 5;
                }

                bitsRemaining -= 3;
                nextChar = (byte)((b << bitsRemaining) & 31);
            }

            // if we didn't end with a full char
            if (arrayIndex == charCount) return new string(returnArray);
            returnArray[arrayIndex++] = ValueToChar(nextChar);
            while(arrayIndex != charCount) returnArray[arrayIndex++] = '='; // padding

            return new string(returnArray);
        }

        private static int CharToValue(char c)
        {
            int value = c;

            return value switch
            {
                // 65-90 == uppercase letters
                < 91 and > 64 => value - 65,
                // 50-55 == numbers 2-7
                < 56 and > 49 => value - 24,
                // 97-122 == lowercase letters
                < 123 and > 96 => value - 97,
                _ => throw new ArgumentException(@"Character is not a Base32 character.", nameof(c)),
            };
        }

        private static char ValueToChar(byte b)
        {
            return b switch
            {
                < 26 => (char)(b + 65),
                < 32 => (char)(b + 24),
                _ => throw new ArgumentException(@"Byte is not a Base32 value.", nameof(b)),
            };
        }
    }
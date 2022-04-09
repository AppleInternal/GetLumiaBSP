﻿// Copyright (c) 2018, Gustave M. - gus33000.me - @gus33000
// Copyright (c) 2017, Rene Lergner - wpinternals.net - @Heathcliff74xda
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace RTInstaller
{
    internal class HandleFile
    {
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }


        public static byte[] GetProperBytes(string location, out bool patched)
        {
            byte[]? data = File.ReadAllBytes(location);

            string? productsuite = "50 00 72 00 6F 00 64 00 75 00 63 00 74 00 53 00 75 00 69 00 74 00 65 00".Replace(" ", "");
            byte[]? productarr = StringToByteArrayFastest(productsuite);
            string? anothersuite = "41 00 6E 00 6F 00 74 00 68 00 65 00 72 00 53 00 75 00 69 00 74 00 65 00".Replace(" ", "");
            byte[]? anotherarr = StringToByteArrayFastest(anothersuite);

            patched = false;

            foreach (int position in data.Locate(productarr))
            {
                patched = true;
                Console.WriteLine("(patcher) Patching " + location + " at " + position);

                for (int i = 0; i < anotherarr.Length; i++)
                {
                    data[i + position] = anotherarr[i];
                }
            }

            if (patched)
            {
                Console.WriteLine("(patcher) Recalculating checksum for " + location);
                CalculateChecksum(data);
            }

            return data;
        }

        private static uint CalculateChecksum(byte[] PEFile)
        {
            uint Checksum = 0;
            uint Hi;

            // Clear file checksum
            WriteUInt32(PEFile, GetChecksumOffset(PEFile), 0);

            for (uint i = 0; i < ((uint)PEFile.Length & 0xfffffffe); i += 2)
            {
                Checksum += ReadUInt16(PEFile, i);
                Hi = Checksum >> 16;
                if (Hi != 0)
                {
                    Checksum = Hi + (Checksum & 0xFFFF);
                }
            }
            if ((PEFile.Length % 2) != 0)
            {
                Checksum += ReadUInt8(PEFile, (uint)PEFile.Length - 1);
                Hi = Checksum >> 16;
                if (Hi != 0)
                {
                    Checksum = Hi + (Checksum & 0xFFFF);
                }
            }
            Checksum += (uint)PEFile.Length;

            // Write file checksum
            WriteUInt32(PEFile, GetChecksumOffset(PEFile), Checksum);

            return Checksum;
        }

        private static uint GetChecksumOffset(byte[] PEFile)
        {
            return ReadUInt32(PEFile, 0x3C) + +0x58;
        }

        internal static uint ReadUInt32(byte[] ByteArray, uint Offset)
        {
            // Assume CPU and FFU are both Little Endian
            return BitConverter.ToUInt32(ByteArray, (int)Offset);
        }

        internal static void WriteUInt32(byte[] ByteArray, uint Offset, uint Value)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(Value), 0, ByteArray, (int)Offset, 4);
        }

        internal static ushort ReadUInt16(byte[] ByteArray, uint Offset)
        {
            // Assume CPU and FFU are both Little Endian
            return BitConverter.ToUInt16(ByteArray, (int)Offset);
        }

        internal static void WriteUInt16(byte[] ByteArray, uint Offset, ushort Value)
        {
            System.Buffer.BlockCopy(BitConverter.GetBytes(Value), 0, ByteArray, (int)Offset, 2);
        }

        internal static byte ReadUInt8(byte[] ByteArray, uint Offset)
        {
            return ByteArray[Offset];
        }

        internal static void WriteUInt8(byte[] ByteArray, uint Offset, byte Value)
        {
            ByteArray[Offset] = Value;
        }
    }
}

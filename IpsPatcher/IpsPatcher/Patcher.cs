//
//   Copyright (C) 2018 Pharap (@Pharap)
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.IO;
using System.Text;

namespace IpsPatcher
{
    public static class Patcher
    {
        private static readonly byte[] PatchString = Encoding.ASCII.GetBytes("PATCH");
        private static readonly byte[] EofString = Encoding.ASCII.GetBytes("EOF");

        public static void Patch(Stream input, Stream output)
        {
            if (!input.CanRead)
                throw new ArgumentException("inStream must be readable");
            if (!output.CanWrite)
                throw new ArgumentException("outStream must be writeable");
            if (!output.CanSeek)
                throw new ArgumentException("outStream must be seekable");

            {
                byte[] patchBuffer = new byte[5];
                input.Read(patchBuffer, 0, 5);
                if (!Equal(patchBuffer, PatchString))
                    throw new FormatException("inStream wrong format");
            }

            while (input.Position < input.Length && input.Length - input.Position > 3)
                ReadPacket(input, output);

            {
                byte[] eof = new byte[3];
                input.Read(eof, 0, 3);
                if (!Equal(eof, EofString))
                    throw new FormatException("inStream not terminated");
            }
        }

        private static bool Equal(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                return false;

            for (int i = 0; i < left.Length; ++i)
                if (left[i] != right[i])
                    return false;

            return true;
        }

        private static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }

        private static void SwapEndian(byte[] data)
        {
            for (int i = 0, j = data.Length - 1; i < j; ++i, --j)
                Swap(ref data[i], ref data[j]);
        }

        private static void ReadPacket(Stream input, Stream output)
        {
            {
                byte[] offsetBuffer = new byte[4];
                input.Read(offsetBuffer, 1, 3);
                if (BitConverter.IsLittleEndian)
                    SwapEndian(offsetBuffer);
                int offset = BitConverter.ToInt32(offsetBuffer, 0);
                output.Seek(offset, SeekOrigin.Begin);
            }

            {
                byte[] lengthBuffer = new byte[2];
                input.Read(lengthBuffer, 0, 2);
                if (BitConverter.IsLittleEndian)
                    SwapEndian(lengthBuffer);
                int length = BitConverter.ToUInt16(lengthBuffer, 0);

                if (length > 0)
                {
                    byte[] dataBuffer = new byte[4096];
                    int remaining = length;
                    while (remaining > 0)
                    {
                        int amount = input.Read(dataBuffer, 0, Math.Min(remaining, dataBuffer.Length));
                        output.Write(dataBuffer, 0, amount);
                        remaining -= amount;
                    }
                    return;
                }
            }

            {
                byte[] repeatBuffer = new byte[2];
                input.Read(repeatBuffer, 0, 2);
                if (BitConverter.IsLittleEndian)
                    SwapEndian(repeatBuffer);
                int repeat = BitConverter.ToUInt16(repeatBuffer, 0);

                int eof = input.ReadByte();
                if (eof == -1)
                    throw new EndOfStreamException();

                byte value = (byte)eof;

                for (int i = 0; i < repeat; ++i)
                    output.WriteByte(value);
            }
        }
    }
}

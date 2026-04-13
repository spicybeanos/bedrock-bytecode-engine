using System;
using System.Collections.Generic;
using System.Text;

namespace BedrockBytecodeEngine
{
    public static class Utils
    {
        private const ulong FNV_OFFSET_BASIS = 0xcbf29ce484222325;
        private const ulong FNV_PRIME = 0x00000100000001B3;

        public static ulong HashStringVm(string str)
        {
            return Fnv1a64(Encoding.UTF8.GetBytes(str));
        }

        public static ulong HashU64Vm(ulong num)
        {
            byte[] bytes = BitConverter.GetBytes(num);
            return Fnv1a64(bytes);
        }

        public static ulong HashBlobVm(byte[] data)
        {
            return Fnv1a64(data);
        }

        public static ulong ReadU64(byte[] data, int offset)
        {
            return BitConverter.ToUInt64(data, offset);
        }

        public static double ReadF64(byte[] data, int offset)
        {
            return BitConverter.ToDouble(data, offset);
        }

        public static long ReadI64(byte[] data, int offset)
        {
            return BitConverter.ToInt64(data, offset);
        }

        public static void WriteU64(byte[] data, int offset, ulong value)
        {
            Array.Copy(BitConverter.GetBytes(value), 0, data, offset, 8);
        }

        public static string ReadString(byte[] data, int offset, int len)
        {
            return Encoding.UTF8.GetString(data, offset, len);
        }

        public static void WriteString(byte[] data, int offset, string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
        }

        private static ulong Fnv1a64(byte[] data)
        {
            ulong hash = FNV_OFFSET_BASIS;
            foreach (byte b in data)
            {
                hash ^= b;
                hash = unchecked(hash * FNV_PRIME);
            }
            return hash;
        }
    }
}

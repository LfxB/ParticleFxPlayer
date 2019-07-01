using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GTA.Math;

namespace Memory
{
    /// <summary>   
    ///     Credits to CamxxCore. IQ 195
    /// </summary>
    internal static class MemoryAccess
    {
        public sealed class Pattern
        {
            private readonly string bytes, mask;
            private readonly IntPtr result;

            public Pattern(string bytes, string mask, string moduleName = null)
            {
                this.bytes = bytes;
                this.mask = mask;
                this.result = FindPattern(moduleName);
            }

            private unsafe IntPtr FindPattern(string moduleName)
            {
                Win32Native.MODULEINFO module;

                Win32Native.GetModuleInformation(
                    Win32Native.GetCurrentProcess(),
                    Win32Native.GetModuleHandle(moduleName),
                    out module,
                    sizeof(Win32Native.MODULEINFO));

                var address = module.lpBaseOfDll.ToInt64();

                var end = address + module.SizeOfImage;

                for (; address < end; address++)
                {
                    if (bCompare((byte*)(address), bytes.ToCharArray(), mask.ToCharArray()))
                    {
                        return new IntPtr(address);
                    }
                }

                return IntPtr.Zero;
            }

            public IntPtr Get(int offset = 0)
            {
                return result + offset;
            }

            private static unsafe bool bCompare(byte* pData, char[] bMask, char[] szMask)
            {
                return !bMask.Where((t, i) => szMask[i] == 'x' && pData[i] != t).Any();
            }
        }

        public static class Win32Native
        {
            [DllImport("kernel32.dll")]
            public static extern IntPtr GetCurrentProcess();

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("psapi.dll", SetLastError = true)]
            public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, int cb);

            [StructLayout(LayoutKind.Sequential)]
            public struct MODULEINFO
            {
                public IntPtr lpBaseOfDll;
                public uint SizeOfImage;
                public IntPtr EntryPoint;
            }
        }

        internal static sbyte ReadSByte(IntPtr address)
        {
            unsafe
            {
                var data = (sbyte*)address.ToPointer();

                return *data;
            }
        }
        internal static byte ReadByte(IntPtr address)
        {
            unsafe
            {
                var data = (byte*)(address.ToPointer());

                return *data;
            }
        }
        internal static short ReadShort(IntPtr address)
        {
            unsafe
            {
                var data = (short*)(address.ToPointer());

                return *data;
            }
        }
        internal static ushort ReadUShort(IntPtr address)
        {
            unsafe
            {
                var data = (ushort*)(address.ToPointer());

                return *data;
            }
        }
        internal static int ReadInt(IntPtr address)
        {
            unsafe
            {
                var data = (int*)(address.ToPointer());

                return *data;
            }
        }
        internal static uint ReadUInt(IntPtr address)
        {
            unsafe
            {
                var data = (uint*)(address.ToPointer());

                return *data;
            }
        }
        internal static float ReadFloat(IntPtr address)
        {
            unsafe
            {
                var data = (float*)(address.ToPointer());

                return *data;
            }
        }
        internal static Vector3 ReadVector3(IntPtr address)
        {
            unsafe
            {
                var data = (float*)(address.ToPointer());

                return new Vector3(data[0], data[1], data[2]);
            }
        }
        internal static String ReadString(IntPtr address)
        {
            return PtrToStringUTF8(address);
        }
        internal static IntPtr ReadPtr(IntPtr address)
        {
            unsafe
            {
                var data = (void**)(address.ToPointer());

                return new IntPtr(*data);
            }
        }
        internal static Matrix ReadMatrix(IntPtr address)
        {
            unsafe
            {
                var data = (Matrix*)(address.ToPointer());

                return *data;
            }

        }
        internal static long ReadLong(IntPtr address)
        {
            unsafe
            {
                var data = (long*)(address.ToPointer());

                return *data;
            }

        }
        internal static ulong ReadULong(IntPtr address)
        {
            unsafe
            {
                var data = (ulong*)(address.ToPointer());

                return *data;
            }
        }
        internal static void WriteSByte(IntPtr address, sbyte value)
        {
            unsafe
            {
                var data = (sbyte*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteByte(IntPtr address, byte value)
        {
            unsafe
            {
                var data = (byte*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteShort(IntPtr address, short value)
        {
            unsafe
            {
                var data = (short*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteUShort(IntPtr address, ushort value)
        {
            unsafe
            {
                var data = (ushort*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteInt(IntPtr address, int value)
        {
            unsafe
            {
                var data = (int*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteUInt(IntPtr address, uint value)
        {
            unsafe
            {
                var data = (uint*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteFloat(IntPtr address, float value)
        {
            unsafe
            {
                var data = (float*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteVector3(IntPtr address, Vector3 value)
        {
            unsafe
            {
                var data = (float*)(address.ToPointer());

                data[0] = value.X;
                data[1] = value.Y;
                data[2] = value.Z;
            }
        }
        internal static void WriteMatrix(IntPtr address, Matrix value)
        {
            unsafe
            {
                var data = (float*)(address.ToPointer());

                var arr = value.ToArray();
                for (int i = 0; i < arr.Length; i++)
                {
                    data[i] = arr[i];
                }
            }
        }
        internal static void WriteLong(IntPtr address, long value)
        {
            unsafe
            {
                var data = (long*)(address.ToPointer());

                *data = value;
            }
        }
        internal static void WriteULong(IntPtr address, ulong value)
        {
            unsafe
            {
                var data = (ulong*)(address.ToPointer());

                *data = value;
            }
        }
        internal static string PtrToStringUTF8(IntPtr ptr)
        {
            if (IntPtr.Zero == ptr)
            {
                return null;
            }
            unsafe
            {
                byte* address = (byte*)ptr.ToPointer();
                int len = 0;

                while (address[len] != 0)
                    ++len;

                return PtrToStringUTF8(ptr, len);
            }
        }
        internal static String PtrToStringUTF8(IntPtr ptr, int byteLen)
        {
            if (byteLen < 0)
            {
                throw new ArgumentException(null, nameof(byteLen));
            }
            else if (IntPtr.Zero == ptr)
            {
                return null;
            }
            else if (byteLen == 0)
            {
                return string.Empty;
            }
            else
            {
                return string.Empty;
            }
            /*else
            {
                byte* pByte = (byte*)ptr.ToPointer();
                return System.Text.Encoding.UTF8.GetString(pByte, byteLen);
            }*/
        }
    }
}
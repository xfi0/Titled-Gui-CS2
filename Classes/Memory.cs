using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace Titled_Gui.Classes.Memory
{
    public class Memory
    {
        public Memory(string processName)
        {
            GetProcess(processName);
        }

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpBytesRead);

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpBytesWritten);

        public static Process process;
        private static IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// Initializes the library pretty much
        /// </summary>
        /// <param name="procname"></param>
        /// <returns></returns>
        public Process GetProcess(string procname)
        {
            process = Renderer.CS2ProcessId != 0 ? Process.GetProcessById(Renderer.CS2ProcessId) : Process.GetProcessesByName(procname)[0];
            handle = process.Handle;
            return process;
        }

        public IntPtr GetModuleBase(string modulename)
        {
            if (string.IsNullOrEmpty(modulename))
            {
                return IntPtr.Zero;
            }

            if (process == null)
            {
                return IntPtr.Zero;
            }

            try
            {
                if (modulename.Contains(".exe"))
                {
                    return process.MainModule.BaseAddress;
                }

                foreach (ProcessModule module in process.Modules)
                {
                    if (module.ModuleName == modulename)
                    {
                        return module.BaseAddress;
                    }
                }
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        public T Read<T>(IntPtr addr) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];

            ReadProcessMemory(handle, addr, buf, size, out _);

            var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
            T value = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());
            h.Free();

            return value;
        }

        public T Read<T>(ulong addr) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];

            ReadProcessMemory(handle, (IntPtr)addr, buf, size, out _);

            var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
            T value = Marshal.PtrToStructure<T>(h.AddrOfPinnedObject());
            h.Free();

            return value;
        }

        public void Write<T>(IntPtr addr, T val) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];
            var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
            Marshal.StructureToPtr(val, h.AddrOfPinnedObject(), false);
            h.Free();
            WriteProcessMemory(handle, addr, buf, size, out _);
        }
        public void Write<T>(ulong addr, T val) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] buf = new byte[size];
            var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
            Marshal.StructureToPtr(val, h.AddrOfPinnedObject(), false);
            h.Free();
            WriteProcessMemory(handle, (IntPtr)addr, buf, size, out _);
        }

        public string ReadString(IntPtr address, nint offset) => ReadString((address + offset));

        public string ReadString(IntPtr address, int maxLength = 256)
        {
            if (address == IntPtr.Zero)
                return "Unknown";

            byte[] buf = new byte[maxLength];

            ReadProcessMemory(handle, address, buf, maxLength, out _);

            int nullTerm = Array.IndexOf(buf, (byte)0);

            return Encoding.UTF8.GetString(buf, 0, nullTerm < 0 ? maxLength : nullTerm);
        }

        public byte[] ReadBytes(IntPtr address, int size)
        {
            byte[] buf = new byte[size];
            ReadProcessMemory(handle, address, buf, size, out _);
            return buf;
        }
        public void WriteBytes(IntPtr address, byte[] buf) => WriteProcessMemory(handle, address, buf, buf.Length, out _);

        public IntPtr ReadPointer(IntPtr address) => (IntPtr)Read<long>(address);
        public IntPtr ReadPointer(IntPtr address, int offset) => ReadPointer(address + offset);
        public int ReadInt(IntPtr address) => Read<int>(address);
        public int ReadInt(IntPtr address, int offset) => Read<int>(address + offset);
        public uint ReadUInt(IntPtr address) => Read<uint>(address);
        public uint ReadUInt(IntPtr address, int offset) => Read<uint>(address + offset);
        public float ReadFloat(IntPtr address) => Read<float>(address);
        public float ReadFloat(IntPtr address, int offset) => Read<float>(address + offset);
        public bool ReadBool(IntPtr address) => Read<byte>(address) != 0;
        public bool ReadBool(IntPtr address, int offset) => Read<byte>(address + offset) != 0;
        public short ReadShort(IntPtr address) => Read<short>(address);
        public short ReadShort(IntPtr address, int offset) => Read<short>(address + offset);
        public char ReadChar(IntPtr address) => Read<char>(address);
        public char ReadChar(IntPtr address, int offset) => Read<char>(address + offset);
        public ulong ReadULong(IntPtr address) => Read<ulong>(address);
        public ulong ReadULong(IntPtr address, int offset) => Read<ulong>(address + offset);
        public void WritePointer(IntPtr address, IntPtr value) => Write<long>(address, (long)value);
        public void WritePointer(IntPtr address, int offset, IntPtr value) => WritePointer(address + offset, value);
        public void WriteInt(IntPtr address, int value) => Write<int>(address, value);
        public void WriteInt(IntPtr address, int offset, int value) => Write<int>(address + offset, value);
        public void WriteUInt(IntPtr address, uint value) => Write<uint>(address, value);
        public void WriteUInt(IntPtr address, int offset, uint value) => Write<uint>(address + offset, value);
        public void WriteFloat(IntPtr address, float value) => Write<float>(address, value);
        public void WriteFloat(IntPtr address, int offset, float value) => Write<float>(address + offset, value);
        public void WriteBool(IntPtr address, bool value) => Write<byte>(address, value ? (byte)1 : (byte)0);
        public void WriteBool(IntPtr address, int offset, bool value) => Write<byte>(address + offset, value ? (byte)1 : (byte)0);
        public void WriteShort(IntPtr address, short value) => Write<short>(address, value);
        public void WriteShort(IntPtr address, int offset, short value) => Write<short>(address + offset, value);
        public void WriteChar(IntPtr address, char value) => Write<char>(address, value);
        public void WriteChar(IntPtr address, int offset, char value) => Write<char>(address + offset, value);
        public void WriteULong(IntPtr address, ulong value) => Write<ulong>(address, value);
        public void WriteULong(IntPtr address, int offset, ulong value) => Write<ulong>(address + offset, value);


        public Vector3 ReadVec(IntPtr address)
        {
            byte[] b = ReadBytes(address, 12);
            return new Vector3(BitConverter.ToSingle(b, 0), BitConverter.ToSingle(b, 4), BitConverter.ToSingle(b, 8));
        }

        public Vector3 ReadVec(IntPtr address, int offset) => ReadVec(address + offset);

        public float[] ReadMatrix(IntPtr address)
        {
            byte[] b = ReadBytes(address, 64);
            float[] m = new float[16];
            for (int i = 0; i < 16; i++)
                m[i] = BitConverter.ToSingle(b, i * 4);
            return m;
        }
    }
}
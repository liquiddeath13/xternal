using System;
using System.Collections.Generic;
using System.Text;
using System.Security;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace xternal
{
    public class NativeAPI
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public int BaseAddress;
            public int AllocationBase;
            public int AllocationProtect;
            public int RegionSize;
            public int State;
            public int Protect;
            public int Type;
        }
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }
        [Flags]
        public enum DuplicateOptions : uint
        {
            DUPLICATE_CLOSE_SOURCE = 0x00000001,
            DUPLICATE_SAME_ACCESS = 0x00000002
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }
        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0X2000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0X200000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }
        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0X20,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0X200
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
           IntPtr hProcess,
           IntPtr lpBaseAddress,
           [Out] byte[] lpBuffer,
           int dwSize,
           out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
           IntPtr hProcess,
           IntPtr lpBaseAddress,
           [Out] byte[] lpBuffer,
           int dwSize,
           out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, DuplicateOptions dwOptions);
        [DllImport("psapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern int EnumProcessModules(IntPtr hProcess, [Out] IntPtr lphModule, uint cb, out uint lpcbNeeded);
        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] int nSize);
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags) => OpenProcess(flags, false, proc.Id);
        public static IntPtr AllAccessHandle(Process proc)
        {
            IntPtr DupHandle = OpenProcess(proc, ProcessAccessFlags.DuplicateHandle);
            IntPtr CurrProcess = new IntPtr(-1);
            DuplicateHandle(DupHandle, CurrProcess, CurrProcess, out IntPtr AllAccessHandle, 0, false, DuplicateOptions.DUPLICATE_SAME_ACCESS);
            return AllAccessHandle;
        }
        public static string GetModulePath(IntPtr hProc, IntPtr module)
        {
            StringBuilder strbld = new StringBuilder(1024);
            GetModuleFileNameEx(hProc, module, strbld, strbld.Capacity);
            return strbld.ToString();
        }
        public static List<RemoteProcessModule> GetProcessModules(IntPtr hProc)
        {
            List<RemoteProcessModule> modulesList = new List<RemoteProcessModule>();
            IntPtr[] hMods = new IntPtr[1024];

            GCHandle gch = GCHandle.Alloc(hMods, GCHandleType.Pinned);
            IntPtr pModules = gch.AddrOfPinnedObject();

            uint uiSize = (uint)(Marshal.SizeOf(typeof(IntPtr)) * (hMods.Length));

            if (EnumProcessModules(hProc, pModules, uiSize, out uint cbNeeded) == 1)
            {
                int uiTotalNumberofModules = (int)(cbNeeded / (Marshal.SizeOf(typeof(IntPtr))));

                for (int i = 0; i < uiTotalNumberofModules; i++)
                {
                    StringBuilder strbld = new StringBuilder(1024);
                    GetModuleFileNameEx(hProc, hMods[i], strbld, strbld.Capacity);
                    GetModuleInformation(hProc, hMods[i], out MODULEINFO modinfo, (uint)Marshal.SizeOf(typeof(MODULEINFO)));
                    modulesList.Add(new RemoteProcessModule
                    {
                        Name = strbld.ToString(),
                        Entry = hMods[i],
                        Size = modinfo.SizeOfImage
                    });
                }
            }
            gch.Free();
            return modulesList;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);
        [DllImport("kernel32.dll")]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, AllocationType dwFreeType);
        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, uint lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    }
}

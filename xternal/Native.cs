using Reloaded.Memory.Sigscan;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace xternal
{
    public class Native
    {
        string NProc { get; set; }
        IntPtr HProc { get; set; }

        readonly Dictionary<string, byte[]> CachedModules = new Dictionary<string, byte[]>();
        public class Pattern
        {
            public string ModuleName { get; set; }
            public string Name { get; set; }
            public string Content { get; set; }
        }
        public Native(string procName)
        {
            NProc = procName;
            HProc = IntPtr.Zero;
            SetProcHandle();
        }
        private bool SetProcHandle()
        {
            var rList = Process.GetProcesses().Where(x => x.ProcessName == NProc);
            if (rList.Count() > 0) HProc = NativeAPI.AllAccessHandle(rList.First());
            return HProc != IntPtr.Zero;
        }
        public bool IsProcessInited() => NProc != "" && HProc != IntPtr.Zero;
        public bool TryUpdateHandle() => SetProcHandle();
        public string GetMainPath() => NativeAPI.GetModulePath(HProc, GetRemoteModuleByName(NProc).Entry);
        public RemoteProcessModule GetRemoteModuleByName(string name, bool needWait = false)
        {
            var result = NativeAPI.GetProcessModules(HProc).Where(x => x.Name.ToLower().Contains(name));
            if (result.Count() > 0) return result.First();
            else if (needWait)
            {
                Thread.Sleep(1000);
                return GetRemoteModuleByName(name);
            }
            else return null;
        }
        public void PrintModules()
        {
            foreach (var module in NativeAPI.GetProcessModules(HProc)) Console.WriteLine($"{module.Name}");
        }
        public uint CallFunction(IntPtr functionPtr, IntPtr param)
        {
            IntPtr hThread = NativeAPI.CreateRemoteThread(HProc, IntPtr.Zero, 0, functionPtr, param, 0, out _);
            NativeAPI.WaitForSingleObject(hThread, 0xFFFFFFFF);
            NativeAPI.GetExitCodeThread(hThread, out uint returnValue);
            return returnValue;
        }
        public bool ModuleExist(string name) => NativeAPI.GetProcessModules(HProc).Where(x => x.Name.Contains(name)).Count() > 0;
        public byte[] ReadMem(IntPtr BaseAddress, int dwSize = 4)
        {
            byte[] valueRead = new byte[dwSize];
            NativeAPI.ReadProcessMemory(HProc, BaseAddress, valueRead, dwSize, out IntPtr n);
            return valueRead;
        }
        public void WriteMemFloat(IntPtr lpBaseAddress, float value, int dwSize)
        {
            byte[] valueByteArray = BitConverter.GetBytes(value);
            if (!NativeAPI.WriteProcessMemory(HProc, lpBaseAddress, valueByteArray, dwSize, out IntPtr numberBytesRW)) Console.WriteLine($"GetLastWin32Error: {Marshal.GetLastWin32Error()}");
        }
        public void WriteMem<T>(IntPtr lpBaseAddress, T value) where T : struct
        {
            byte[] valueByteArray = GetBytes(value);
            if (!NativeAPI.WriteProcessMemory(HProc, lpBaseAddress, valueByteArray, Marshal.SizeOf(value), out IntPtr numberBytesRW)) Console.WriteLine($"GetLastWin32Error: {Marshal.GetLastWin32Error()}");
        }
        internal byte[] GetBytes<T>(T value)
        {
            int size = Marshal.SizeOf(value);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }
        internal unsafe T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            fixed (byte* ptr = &bytes[0])
            {
                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }
        internal object FindPatternInModule(Pattern pattern, bool debugOutput = true)
        {
            RemoteProcessModule rm = GetRemoteModuleByName(pattern.ModuleName);
            if (rm != null)
            {
                if (debugOutput) Console.WriteLine($"searching in {rm.Name} (base: {rm.Entry}; size: {rm.Size})..");
                if (!CachedModules.ContainsKey(rm.Name)) CachedModules.Add(rm.Name, ReadMem(rm.Entry, (int)rm.Size));
                byte[] ModuleMem = CachedModules[rm.Name];
                var result = new Scanner(ModuleMem).CompiledFindPattern(pattern.Content.Replace("?", "??"));
                return result;
            }
            return null;
        }
        internal IntPtr AllocMem(uint dwSize) => NativeAPI.VirtualAllocEx(HProc, IntPtr.Zero, dwSize, NativeAPI.AllocationType.Commit, NativeAPI.MemoryProtection.ExecuteReadWrite);
        internal object FindModuleByAddressInRange(int address) => NativeAPI.GetProcessModules(HProc).FirstOrDefault(module => address > (int)module.Entry && address < ((int)module.Entry + (int)module.Size));
    }
}

using Reloaded.Memory.Sigscan.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace xternal
{
    public class CrossFireEngine : Native
    {
        internal IntPtr pLTClientShell = IntPtr.Zero;
        public CrossFireEngine() : base("crossfire")
        {
            if (IsProcessInited()) Init();
        }
        private void Init()
        {
            var cshellModule = GetRemoteModuleByName("cshell");
            while (cshellModule == null) cshellModule = GetRemoteModuleByName("cshell");
            var pLTClientShellAddress = BitConverter.ToInt32(ReadMem(IntPtr.Add(cshellModule.Entry, 0x1BFF438)), 0);
            pLTClientShell = IntPtr.Add(IntPtr.Zero, pLTClientShellAddress);
        }
        public IntPtr CopyModelNodes(int modelnodeptr)
        {
            List<byte> ModelNodesInfoArray = new List<byte>();
            for (int i = 0; i < 2500; i++)
            {
                IntPtr NodePtr = GetModelNodePtrAddress(modelnodeptr, i);
                if (NodePtr != IntPtr.Zero) ModelNodesInfoArray.AddRange(ReadMem(NodePtr, 0x9C));
            }
            var AllocatedMemStartPtr = AllocMem((uint)ModelNodesInfoArray.Count);
            for (int i = 0; i < ModelNodesInfoArray.Count; ++i) WriteMem(AllocatedMemStartPtr + i, ModelNodesInfoArray[i]);
            return AllocatedMemStartPtr;
        }
        public IntPtr GetModelNodePtrAddress(int modelnodeptr, int index) => index < 0 || index > 1848 || index > 2499 ? IntPtr.Zero : IntPtr.Add(IntPtr.Zero, 0x9C * index + modelnodeptr);
        public void ReadModelNodeDims(int modelnodeptr, int index)
        {
            Console.WriteLine($"{GetModelNodeName(modelnodeptr, index)}:");
            for (int i = 0; i < 3; ++i) Console.Write($"{ReadValueAtMem<float>(IntPtr.Add(GetModelNodePtrAddress(modelnodeptr, index), 0x38 + i * 4))} ");
            Console.WriteLine();
        }
        public void SetModelNodeDim(int modelnodeptr, int index, float val)
        {
            for (int i = 0; i < 3; ++i) WriteMemFloat(IntPtr.Add(GetModelNodePtrAddress(modelnodeptr, index), 0x38 + i * 4), val, 4);
        }
        public string GetModelNodeName(int modelnodeptr, int index)
        {
            var NodePtr = GetModelNodePtrAddress(modelnodeptr, index);
            return NodePtr != IntPtr.Zero ? Encoding.Default.GetString(ReadMem(IntPtr.Add(NodePtr, 0X2), 32)) : "";
        }
        public object AddressModuleOwner(int address) => FindModuleByAddressInRange(address);
        public int AddressOfPattern(Pattern pattern)
        {
            object result = FindPatternInModule(pattern);
            return result != null ? (int)GetRemoteModuleByName("cshell").Entry + ((PatternScanResult)result).Offset : -1;
        }
        public T ReadValueAtMem<T>(IntPtr Address) where T : struct => ByteArrayToStructure<T>(ReadMem(Address));
        public void WriteValueToMem<T>(IntPtr Address, T val) where T : struct => WriteMem(Address, val);
        public IntPtr WriteBytesToMem(IntPtr Address, List<byte> bytes, bool shouldAlloc)
        {
            if (shouldAlloc) Address = AllocMem((uint)bytes.Count);
            for (int i = 0; i < bytes.Count; i++) WriteMem(Address + i, bytes[i]);
            return Address;
        }
        public void ShowFunc(IntPtr BaseAddress, int dwSize)
        {
            for (int i = 0; i < dwSize; i++)
            {
                if (i % 16 == 0) Console.WriteLine();
                Console.Write($"{ReadMem(BaseAddress + i, 1)[0]:X2} ");
            }
            Console.WriteLine();
        }
        public void CreateDump(List<Pattern> patternsList, bool toConsole = true, bool toFile = false)
        {
            foreach (var pattern in patternsList)
            {
                object result = FindPatternInModule(pattern);
                if (result != null)
                {
                    var psr = (PatternScanResult)result;
                    var str = $"{pattern.Name} : {pattern.Content} : " + (psr.Found ? $"{psr.Offset:X2}" : "not found!");
                    if (toFile) File.AppendAllText($"{DateTime.Now:dd-MM-yyyy_HH-mm-ss}_dump.txt", $"{str}\n");
                    if (toConsole) Console.WriteLine(str);
                }
            }
        }
    }
}

using System;
using System.Text;

namespace xternal
{
    public class PVPLogicHolder : CrossFireEngine
    {
        public PVPLogicHolder() : base() { }
        private IntPtr GetPlayerPtrByIndex(int index) => IntPtr.Add(pLTClientShell, (index * 0xD88) + 0x204);
        private byte GetLocalPlayerIndex()
        {
            int clients = BitConverter.ToInt32(ReadMem(IntPtr.Add(pLTClientShell, 0x204)), 0);
            byte result = (byte)BitConverter.ToInt32(ReadMem(IntPtr.Add(pLTClientShell, clients * 0xD88 + 0x20C)), 0);
            return (byte)(clients < 16 ? result : -1);
        }
        IntPtr GetPlayerObjectPtrAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x4);
        IntPtr GetPlayerClientIDAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x8);
        IntPtr GetPlayerTeamAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x9);
        IntPtr GetPlayerNameAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0xA);
        IntPtr GetPlayerSlotTeamAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x1C);
        IntPtr GetPlayerHasC4Address(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x24);
        IntPtr GetPlayerHealthAddress(int index) => IntPtr.Add(GetPlayerPtrByIndex(index), 0x40);
        public bool IsInFight() => ModuleExist("object") && GetLocalPlayerPos() != null;
        public bool ValidPlayer(int index, bool excludeMe = true, bool enemyOnly = false, bool aliveOnly = true, bool nearOnly = true, float nearRadius = 3500f)
        {
            bool flag = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(ReadMem(GetPlayerObjectPtrAddress(index)), 0)) != IntPtr.Zero;
            if (!flag) return false;
            if (excludeMe)
            {
                flag = GetLocalPlayerClientID() != GetPlayerClientID(index);
                if (!flag) return false;
            }
            if (enemyOnly)
            {
                flag = GetLocalPlayerTeam() != GetPlayerTeam(index);
                if (!flag) return false;
            }
            if (aliveOnly)
            {
                flag = GetPlayerHealth(index) > 0;
                if (!flag) return false;
            }
            if (nearOnly)
            {
                flag = GetLocalPlayerPos().Dist(GetPlayerPos(index)).CompareTo(nearRadius) <= 0;
                if (!flag) return false;
            }
            return true;
        }
        public LTVectorFloat GetPlayerPos(int index)
        {
            LTVectorFloat result = null;
            IntPtr PlayerObjectPtrAddress = GetPlayerObjectPtrAddress(index);
            IntPtr PlayerObjectPtr = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(ReadMem(PlayerObjectPtrAddress), 0));
            if (PlayerObjectPtr != IntPtr.Zero)
            {
                result = new LTVectorFloat
                {
                    X = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xE8)), 0),
                    Y = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xEC)), 0),
                    Z = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xF0)), 0)
                };
            }
            return result;
        }
        public LTRotation GetPlayerRotation(int index)
        {
            LTRotation result = null;
            IntPtr PlayerObjectPtrAddress = GetPlayerObjectPtrAddress(index);
            IntPtr PlayerObjectPtr = IntPtr.Add(IntPtr.Zero, BitConverter.ToInt32(ReadMem(PlayerObjectPtrAddress), 0));
            if (PlayerObjectPtr != IntPtr.Zero)
            {
                result = new LTRotation
                {
                    X = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xF4)), 0),
                    Y = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xF8)), 0),
                    Z = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0xFC)), 0),
                    W = BitConverter.ToSingle(ReadMem(IntPtr.Add(PlayerObjectPtr, 0x100)), 0),
                };
            }
            return result;
        }
        public byte GetPlayerClientID(int index) => (byte)BitConverter.ToInt32(ReadMem(GetPlayerClientIDAddress(index)), 0);
        public byte GetPlayerTeam(int index) => (byte)BitConverter.ToInt32(ReadMem(GetPlayerTeamAddress(index)), 0);
        public string GetPlayerName(int index) => Encoding.Default.GetString(ReadMem(GetPlayerNameAddress(index), 12));
        public int GetPlayerSlotTeam(int index) => BitConverter.ToInt32(ReadMem(GetPlayerSlotTeamAddress(index)), 0);
        public bool GetPlayerHasC4(int index) => BitConverter.ToBoolean(ReadMem(GetPlayerHasC4Address(index)), 0);
        public short GetPlayerHealth(int index) => BitConverter.ToInt16(ReadMem(GetPlayerHealthAddress(index), 2), 0);
        public LTVectorFloat GetLocalPlayerPos() => GetPlayerPos(GetLocalPlayerIndex());
        public LTRotation GetLocalPlayerRotation() => GetPlayerRotation(GetLocalPlayerIndex());
        public byte GetLocalPlayerClientID() => GetPlayerClientID(GetLocalPlayerIndex());
        public byte GetLocalPlayerTeam() => GetPlayerTeam(GetLocalPlayerIndex());
        public string GetLocalPlayerName() => GetPlayerName(GetLocalPlayerIndex());
        public int GetLocalPlayerSlotTeam() => GetPlayerSlotTeam(GetLocalPlayerIndex());
        public bool GetLocalPlayerHasC4() => GetPlayerHasC4(GetLocalPlayerIndex());
        public short GetLocalPlayerHealth() => GetPlayerHealth(GetLocalPlayerIndex());
    }
}

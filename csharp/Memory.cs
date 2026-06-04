
using System;

namespace BedrockBytecodeEngine
{
    public class Memory
    {
        internal static void mov(byte r,VM vm,ulong[] Reg, InstrParam v2)
        {
            Reg[r] = VM.ResolveParam_reg(v2, Reg);
        }
//VM VM
        internal static void read(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!vm.ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k >= buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = buff[k];
        }

        internal static void write(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!vm.ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);
            buff[k] = (byte)(Reg[r] & 0xFF);
        }

        internal static void check(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!vm.ViRam.TryGetValue(addr, out byte[] buff))
            {
                Reg[r] = 0;
                return;
            }

            if (k < buff.Length)
            {
                Reg[r] = 1;
            }
            else
            {
                Reg[r] = 0;
            }
        }

        internal static void alloc(byte r,VM vm,ulong[] Reg, InstrParam sizeP)
        {
            ulong size = VM.ResolveParam_reg(sizeP, Reg);
            ulong handle = Utils.HashU64Vm((ulong)vm.ViRam.Count);
            vm.ViRam[handle] = new byte[size];
            Reg[r] = handle;
        }

        internal static void realloc(byte r,VM vm,ulong[] Reg, InstrParam sizeP)
        {
            ulong handle = Reg[r];
            ulong newSize = VM.ResolveParam_reg(sizeP, Reg);

            if (!vm.ViRam.TryGetValue(handle, out byte[] oldBuff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            byte[] newBuff = new byte[newSize];
            Array.Copy(oldBuff, newBuff, Math.Min(oldBuff.Length, (int)newSize));
            vm.ViRam[handle] = newBuff;
        }

        internal static void free(InstrParam addrP, VM vm, ulong[] Reg)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            vm.ViRam.Remove(addr);
        }

        internal static void vlen(byte r,VM vm,ulong[] Reg, InstrParam locP)
        {
            ulong loc = VM.ResolveParam_reg(locP, Reg);
            if (!vm.ViRam.TryGetValue(loc, out byte[] buff))
            {
                Reg[r] = 0;
                return;
            }

            Reg[r] = (ulong)buff.Length;
        }

        internal static void readw(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!vm.ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = BitConverter.ToUInt64(buff, k);
        }

        internal static void writew(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!vm.ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            byte[] valBytes = BitConverter.GetBytes(Reg[r]);
            Array.Copy(valBytes, 0, buff, k, 8);
        }

        // Memory Operations (Persistent)
        internal static void palloc(byte r,VM vm,ulong[] Reg, InstrParam sizeP)
        {
            ulong size = VM.ResolveParam_reg(sizeP, Reg);
            ulong handle = Utils.HashU64Vm((ulong)GlobalState.Instance.PVram.Count + vm.Id);
            GlobalState.Instance.PVram[handle] = new PVramEntry(vm.Id, new byte[size]);
            Reg[r] = handle;
        }

        internal static void prealloc(byte r,VM vm,ulong[] Reg, InstrParam sizeP)
        {
            ulong handle = Reg[r];
            ulong newSize = VM.ResolveParam_reg(sizeP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(handle, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            if (entry.OwnerId != vm.Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);

            byte[] newBuff = new byte[newSize];
            Array.Copy(entry.Data, newBuff, Math.Min(entry.Data.Length, (int)newSize));
            entry.Data = newBuff;
        }

        internal static void pfree(InstrParam addrP,VM vm, ulong[] Reg)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            if (entry.OwnerId != vm.Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);

            GlobalState.Instance.PVram.Remove(addr);
        }

        internal static void pcheck(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
            {
                Reg[r] = 0;
                return;
            }

            if (k < entry.Data.Length)
            {
                Reg[r] = 1;
            }
            else
            {
                Reg[r] = 0;
            }
        }

        internal static void pread(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k >= entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = entry.Data[k];
        }

        internal static void pwrite(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (entry.OwnerId != vm.Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);
            if (k >= entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);
            if (k + 8 > entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);
            entry.Data[k] = (byte)(Reg[r] & 0xFF);
        }

        internal static void preadw(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = BitConverter.ToUInt64(entry.Data, k);
        }

        internal static void pwritew(byte r,VM vm,ulong[] Reg, InstrParam addrP, InstrParam kP)
        {
            ulong addr = VM.ResolveParam_reg(addrP, Reg);
            int k = (int)VM.ResolveParam_reg(kP, Reg);

            if (!GlobalState.Instance.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (entry.OwnerId != vm.Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);
            if (k + 8 > entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            byte[] valBytes = BitConverter.GetBytes(Reg[r]);
            Array.Copy(valBytes, 0, entry.Data, k, 8);
        }

        internal static void pvlen(byte r, InstrParam locP,ulong[] Reg)
        {
            ulong loc = VM.ResolveParam_reg(locP, Reg);
            if (!GlobalState.Instance.PVram.TryGetValue(loc, out PVramEntry entry))
            {
                Reg[r] = 0;
                return;
            }

            Reg[r] = (ulong)entry.Data.Length;
        }

        internal static void powner(byte r, InstrParam locP,ulong[] Reg)
        {
            ulong loc = VM.ResolveParam_reg(locP, Reg);
            if (!GlobalState.Instance.PVram.TryGetValue(loc, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            Reg[r] = entry.OwnerId;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BedrockBytecodeEngine
{
    public class PVramEntry
    {
        public ulong OwnerId { get; set; }
        public byte[] Data { get; set; }
        public ulong Sequence { get; set; }

        public PVramEntry(ulong ownerId, byte[] data)
        {
            OwnerId = ownerId;
            Data = data;
            Sequence = 0;
        }
    }

    public class GlobalState
    {
        public Dictionary<ulong, PVramEntry> PVram { get; } = new Dictionary<ulong, PVramEntry>();
        public Dictionary<ulong, Dictionary<ulong, ulong>> Attendance { get; } =
            new Dictionary<ulong, Dictionary<ulong, ulong>>();

        public static GlobalState NewShared()
        {
            return new GlobalState();
        }
    }

    public class VM
    {
        private const int MaxStackSize = 0x10000;
        private const int MaxRegisters = 256;

        public ulong Id { get; }
        public ulong[] Reg { get; }
        public int Pc { get; set; }
        public Dictionary<ulong, byte[]> ViRam { get; } = new Dictionary<ulong, byte[]>();
        public Dictionary<ulong, int> LabelIndexs { get; } = new Dictionary<ulong, int>();
        public List<ulong> Stack { get; } = new List<ulong>();
        public List<int> CallStack { get; } = new List<int>();
        public GlobalState Global { get; }

        public VM(ulong id, GlobalState global)
        {
            Id = id;
            Global = global;
            Reg = new ulong[MaxRegisters];
            Pc = 0;
        }

        public ulong Run(List<Instruction> instructions, int start)
        {
            Pc = start;
            int len = instructions.Count;

            while (Pc < len)
            {
                Execute(instructions);
            }

            return 0;
        }

        public void Execute(List<Instruction> instructions)
        {
            Instruction work = instructions[Pc];
            Pc++;

            switch (work)
            {
                // Math
                case Add add:
                    add_impl(add.Reg, add.V2);
                    break;
                case Fadd fadd:
                    fadd_impl(fadd.Reg, fadd.V2);
                    break;
                case Finv finv:
                    finv_impl(finv.Reg, finv.V2);
                    break;
                case Sub sub:
                    sub_impl(sub.Reg, sub.V2);
                    break;
                case Fsub fsub:
                    fsub_impl(fsub.Reg, fsub.V2);
                    break;
                case Mult mult:
                    mult_impl(mult.Reg, mult.V2);
                    break;
                case Fmult fmult:
                    fmult_impl(fmult.Reg, fmult.V2);
                    break;
                case Div div:
                    div_impl(div.Reg, div.V2);
                    break;
                case Fdiv fdiv:
                    fdiv_impl(fdiv.Reg, fdiv.V2);
                    break;
                case Mod mod:
                    modulous_impl(mod.Reg, mod.V2);
                    break;
                case Fmod fmod:
                    fmodulous_impl(fmod.Reg, fmod.V2);
                    break;
                case Lshift lshift:
                    lshift_impl(lshift.Reg, lshift.V2);
                    break;
                case Rshift rshift:
                    rshift_impl(rshift.Reg, rshift.V2);
                    break;
                case Mov mov:
                    mov_impl(mov.Reg, mov.V2);
                    break;
                case Sqrt sqrt:
                    sqrt_impl(sqrt.Reg, sqrt.V2);
                    break;
                case Fsqrt fsqrt:
                    fsqrt_impl(fsqrt.Reg, fsqrt.V2);
                    break;
                case Itof itof:
                    itof_impl(itof.Reg, itof.V2);
                    break;
                case Ftoi ftoi:
                    ftoi_impl(ftoi.Reg, ftoi.V2);
                    break;

                // Comparisons (Int)
                case Eq eq:
                    eq_impl(eq.Reg, eq.V1, eq.V2);
                    break;
                case Neq neq:
                    neq_impl(neq.Reg, neq.V1, neq.V2);
                    break;
                case Gt gt:
                    gt_impl(gt.Reg, gt.V1, gt.V2);
                    break;
                case Lt lt:
                    lt_impl(lt.Reg, lt.V1, lt.V2);
                    break;
                case Gte gte:
                    gte_impl(gte.Reg, gte.V1, gte.V2);
                    break;
                case Lte lte:
                    lte_impl(lte.Reg, lte.V1, lte.V2);
                    break;
                case And and:
                    and_impl(and.Reg, and.V1, and.V2);
                    break;
                case Or or:
                    or_impl(or.Reg, or.V1, or.V2);
                    break;
                case Xor xor:
                    xor_impl(xor.Reg, xor.V1, xor.V2);
                    break;
                case Not not:
                    not_impl(not.Reg, not.V1);
                    break;

                // Comparisons (Float)
                case Feq feq:
                    feq_impl(feq.Reg, feq.V1, feq.V2);
                    break;
                case Fneq fneq:
                    fneq_impl(fneq.Reg, fneq.V1, fneq.V2);
                    break;
                case Fgt fgt:
                    fgt_impl(fgt.Reg, fgt.V1, fgt.V2);
                    break;
                case Flt flt:
                    flt_impl(flt.Reg, flt.V1, flt.V2);
                    break;
                case Fgte fgte:
                    fgte_impl(fgte.Reg, fgte.V1, fgte.V2);
                    break;
                case Flte flte:
                    flte_impl(flte.Reg, flte.V1, flte.V2);
                    break;

                // Memory (Local)
                case Read read:
                    read_impl(read.Reg, read.Addr, read.K);
                    break;
                case Write write:
                    write_impl(write.Reg, write.Addr, write.K);
                    break;
                case Check check:
                    check_impl(check.Reg, check.Addr, check.K);
                    break;
                case Alloc alloc:
                    alloc_impl(alloc.Reg, alloc.Size);
                    break;
                case Realloc realloc:
                    realloc_impl(realloc.Reg, realloc.Size);
                    break;
                case Free free:
                    free_impl(free.Addr);
                    break;
                case Vlen vlen:
                    vlen_impl(vlen.Reg, vlen.Loc);
                    break;
                case ReadW readw:
                    readw_impl(readw.Reg, readw.Addr, readw.K);
                    break;
                case WriteW writew:
                    writew_impl(writew.Reg, writew.Addr, writew.K);
                    break;

                // Memory (Persistent)
                case Palloc palloc:
                    palloc_impl(palloc.Reg, palloc.Size);
                    break;
                case Prealloc prealloc:
                    prealloc_impl(prealloc.Reg, prealloc.Size);
                    break;
                case Pfree pfree:
                    pfree_impl(pfree.Addr);
                    break;
                case Pcheck pcheck:
                    pcheck_impl(pcheck.Reg, pcheck.Addr, pcheck.K);
                    break;
                case Pread pread:
                    pread_impl(pread.Reg, pread.Addr, pread.K);
                    break;
                case Pwrite pwrite:
                    pwrite_impl(pwrite.Reg, pwrite.Addr, pwrite.K);
                    break;
                case PreadW preadw:
                    preadw_impl(preadw.Reg, preadw.Addr, preadw.K);
                    break;
                case PwriteW pwritew:
                    pwritew_impl(pwritew.Reg, pwritew.Addr, pwritew.K);
                    break;
                case Pvlen pvlen:
                    pvlen_impl(pvlen.Reg, pvlen.Loc);
                    break;
                case Powner powner:
                    powner_impl(powner.Reg, powner.Loc);
                    break;

                // Control Flow
                case Jmp jmp:
                    jmp_impl(jmp.JmpHash);
                    break;
                case Jif jif:
                    jif_impl(jif.Reg, jif.JmpHash);
                    break;
                case Jnif jnif:
                    jnif_impl(jnif.Reg, jnif.JmpHash);
                    break;
                case Call call:
                    call_impl(call.JmpHash);
                    break;
                case Ret ret:
                    ret_impl();
                    break;
                case Push push:
                    push_impl(push.Value);
                    break;
                case Pop pop:
                    pop_impl(pop.Reg);
                    break;

                // System
                case SyscallH syscallh:
                    syscallh_impl(syscallh.Hash, syscallh.Handle, syscallh.Dest);
                    break;
                case Syscall1 syscall1:
                    syscall1_impl(syscall1.Hash, syscall1.Arg, syscall1.Dest);
                    break;

                case Pass:
                    break;
            }
        }

        private ulong ResolveParam(InstrParam param)
        {
            return param.Type == InstrParamType.Register ? Reg[param.RegisterId] : param.Value;
        }

        // Math Operations
        private void add_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] += val;
        }

        private void fadd_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) + val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void finv_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = 1.0 / val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void sub_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] -= val;
        }

        private void fsub_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) - val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void mult_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] *= val;
        }

        private void fmult_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) * val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void div_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] /= val;
        }

        private void fdiv_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) / val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void modulous_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] %= val;
        }

        private void fmodulous_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) % val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void lshift_impl(byte r, InstrParam v2)
        {
            int val = (int)ResolveParam(v2);
            Reg[r] <<= val;
        }

        private void rshift_impl(byte r, InstrParam v2)
        {
            int val = (int)ResolveParam(v2);
            Reg[r] >>= val;
        }

        private void sqrt_impl(byte r, InstrParam v2)
        {
            ulong val = ResolveParam(v2);
            Reg[r] = (ulong)Math.Sqrt(val);
        }

        private void fsqrt_impl(byte r, InstrParam v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            double res = Math.Sqrt(val);
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        private void mov_impl(byte r, InstrParam v2)
        {
            Reg[r] = ResolveParam(v2);
        }

        private void itof_impl(byte r, byte v2)
        {
            double val = (double)Reg[v2];
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(val);
        }

        private void ftoi_impl(byte r, byte v2)
        {
            double val = BitConverter.Int64BitsToDouble((long)Reg[v2]);
            Reg[r] = (ulong)(long)val;
        }

        // Comparison Operations (Int)
        private void eq_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 == val2 ? 1UL : 0UL;
        }

        private void neq_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 != val2 ? 1UL : 0UL;
        }

        private void lt_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 < val2 ? 1UL : 0UL;
        }

        private void gt_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 > val2 ? 1UL : 0UL;
        }

        private void lte_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 <= val2 ? 1UL : 0UL;
        }

        private void gte_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 >= val2 ? 1UL : 0UL;
        }

        private void and_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 & val2;
        }

        private void or_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 | val2;
        }

        private void xor_impl(byte r, InstrParam v1, InstrParam v2)
        {
            ulong val1 = ResolveParam(v1);
            ulong val2 = ResolveParam(v2);
            Reg[r] = val1 ^ val2;
        }

        private void not_impl(byte r, InstrParam v1)
        {
            ulong val = ResolveParam(v1);
            Reg[r] = ~val;
        }

        // Comparison Operations (Float)
        private void feq_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 == val2 ? 1UL : 0UL;
        }

        private void fneq_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 != val2 ? 1UL : 0UL;
        }

        private void flt_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 < val2 ? 1UL : 0UL;
        }

        private void fgt_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 > val2 ? 1UL : 0UL;
        }

        private void flte_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 <= val2 ? 1UL : 0UL;
        }

        private void fgte_impl(byte r, InstrParam v1, InstrParam v2)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)ResolveParam(v1));
            double val2 = BitConverter.Int64BitsToDouble((long)ResolveParam(v2));
            Reg[r] = val1 >= val2 ? 1UL : 0UL;
        }

        // Memory Operations (Local)
        private void read_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k >= buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = buff[k];
        }

        private void write_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            buff[k] = (byte)(Reg[r] & 0xFF);
        }

        private void check_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!ViRam.TryGetValue(addr, out byte[] buff))
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

        private void alloc_impl(byte r, InstrParam sizeP)
        {
            ulong size = ResolveParam(sizeP);
            ulong handle = Utils.HashU64Vm((ulong)ViRam.Count);
            ViRam[handle] = new byte[size];
            Reg[r] = handle;
        }

        private void realloc_impl(byte r, InstrParam sizeP)
        {
            ulong handle = Reg[r];
            ulong newSize = ResolveParam(sizeP);

            if (!ViRam.TryGetValue(handle, out byte[] oldBuff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            byte[] newBuff = new byte[newSize];
            Array.Copy(oldBuff, newBuff, Math.Min(oldBuff.Length, (int)newSize));
            ViRam[handle] = newBuff;
        }

        private void free_impl(InstrParam addrP)
        {
            ulong addr = ResolveParam(addrP);
            ViRam.Remove(addr);
        }

        private void vlen_impl(byte r, InstrParam locP)
        {
            ulong loc = ResolveParam(locP);
            if (!ViRam.TryGetValue(loc, out byte[] buff))
            {
                Reg[r] = 0;
                return;
            }

            Reg[r] = (ulong)buff.Length;
        }

        private void readw_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = BitConverter.ToUInt64(buff, k);
        }

        private void writew_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > buff.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            byte[] valBytes = BitConverter.GetBytes(Reg[r]);
            Array.Copy(valBytes, 0, buff, k, 8);
        }

        // Memory Operations (Persistent)
        private void palloc_impl(byte r, InstrParam sizeP)
        {
            ulong size = ResolveParam(sizeP);
            ulong handle = Utils.HashU64Vm((ulong)Global.PVram.Count + Id);
            Global.PVram[handle] = new PVramEntry(Id, new byte[size]);
            Reg[r] = handle;
        }

        private void prealloc_impl(byte r, InstrParam sizeP)
        {
            ulong handle = Reg[r];
            ulong newSize = ResolveParam(sizeP);

            if (!Global.PVram.TryGetValue(handle, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            if (entry.OwnerId != Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);

            byte[] newBuff = new byte[newSize];
            Array.Copy(entry.Data, newBuff, Math.Min(entry.Data.Length, (int)newSize));
            entry.Data = newBuff;
        }

        private void pfree_impl(InstrParam addrP)
        {
            ulong addr = ResolveParam(addrP);
            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            if (entry.OwnerId != Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);

            Global.PVram.Remove(addr);
        }

        private void pcheck_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
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

        private void pread_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k >= entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = entry.Data[k];
        }

        private void pwrite_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (entry.OwnerId != Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);
            if (k >= entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            entry.Data[k] = (byte)(Reg[r] & 0xFF);
        }

        private void preadw_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (k + 8 > entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            Reg[r] = BitConverter.ToUInt64(entry.Data, k);
        }

        private void pwritew_impl(byte r, InstrParam addrP, InstrParam kP)
        {
            ulong addr = ResolveParam(addrP);
            int k = (int)ResolveParam(kP);

            if (!Global.PVram.TryGetValue(addr, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            if (entry.OwnerId != Id)
                throw new RuntimeErrorException(RuntimeError.NotOwnerOfBuffer);
            if (k + 8 > entry.Data.Length)
                throw new RuntimeErrorException(RuntimeError.OutOfBounds);

            byte[] valBytes = BitConverter.GetBytes(Reg[r]);
            Array.Copy(valBytes, 0, entry.Data, k, 8);
        }

        private void pvlen_impl(byte r, InstrParam locP)
        {
            ulong loc = ResolveParam(locP);
            if (!Global.PVram.TryGetValue(loc, out PVramEntry entry))
            {
                Reg[r] = 0;
                return;
            }

            Reg[r] = (ulong)entry.Data.Length;
        }

        private void powner_impl(byte r, InstrParam locP)
        {
            ulong loc = ResolveParam(locP);
            if (!Global.PVram.TryGetValue(loc, out PVramEntry entry))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            Reg[r] = entry.OwnerId;
        }

        // Control Flow
        private void jmp_impl(ulong jmpHash)
        {
            if (!LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            Pc = pc;
        }

        private void jif_impl(byte r, ulong jmpHash)
        {
            if (!LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (Reg[r] == 1)
                Pc = pc;
        }

        private void jnif_impl(byte r, ulong jmpHash)
        {
            if (!LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (Reg[r] != 1)
                Pc = pc;
        }

        private void call_impl(ulong jmpHash)
        {
            if (!LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (CallStack.Count >= MaxStackSize)
                throw new RuntimeErrorException(RuntimeError.CallStackOverflow);
            CallStack.Add(Pc);
            Pc = pc;
        }

        private void ret_impl()
        {
            if (CallStack.Count == 0)
                throw new RuntimeErrorException(RuntimeError.EmptyCallStack);
            Pc = CallStack[CallStack.Count - 1];
            CallStack.RemoveAt(CallStack.Count - 1);
        }

        private void push_impl(InstrParam valueP)
        {
            ulong value = ResolveParam(valueP);
            if (Stack.Count >= MaxStackSize)
                throw new RuntimeErrorException(RuntimeError.StackOverflow);
            Stack.Add(value);
        }

        private void pop_impl(byte r)
        {
            if (Stack.Count == 0)
                throw new RuntimeErrorException(RuntimeError.EmptyStack);
            Reg[r] = Stack[Stack.Count - 1];
            Stack.RemoveAt(Stack.Count - 1);
        }

        // System Calls
        private string ReadBufferString(ulong addr)
        {
            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            string str = Encoding.UTF8.GetString(buff).TrimEnd('\0');
            return str;
        }

        private void WriteBufferBytes(ulong addr, byte[] bytes)
        {
            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            Array.Clear(buff, 0, buff.Length);
            Array.Resize(ref buff, bytes.Length);
            Array.Copy(bytes, buff, bytes.Length);
            ViRam[addr] = buff;
        }

        private void WriteBufferU64(ulong addr, ulong value)
        {
            WriteBufferBytes(addr, BitConverter.GetBytes(value));
        }

        private void syscallh_impl(ulong hash, InstrParam handleP, InstrParam destP)
        {
            ulong handleAddr = ResolveParam(handleP);
            ulong destAddr = ResolveParam(destP);

            switch (hash)
            {
                // io.console.print -> 0x939583C88643DA33
                case 0x939583C88643DA33:
                {
                    string text = ReadBufferString(handleAddr);
                    Console.Write(text);
                    Console.Out.Flush();
                    break;
                }
                // io.console.read_line -> 0x9B1285549BD07847
                case 0x9B1285549BD07847:
                {
                    string prompt = ReadBufferString(handleAddr);
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        Console.Write(prompt);
                        Console.Out.Flush();
                    }
                    string input = Console.ReadLine() ?? "";
                    WriteBufferBytes(destAddr, Encoding.UTF8.GetBytes(input));
                    break;
                }
                // io.file.read -> 0x38E75B427198D6E3
                case 0x38E75B427198D6E3:
                {
                    string path = ReadBufferString(handleAddr);
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(path);
                        WriteBufferBytes(destAddr, bytes);
                    }
                    catch
                    {
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    }
                    break;
                }
                // io.file.write -> 0x5072E3C89F8F6402
                case 0x5072E3C89F8F6402:
                {
                    string path = ReadBufferString(handleAddr);
                    if (!ViRam.TryGetValue(destAddr, out byte[] data))
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    try
                    {
                        File.WriteAllBytes(path, data);
                    }
                    catch
                    {
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    }
                    break;
                }
                // io.file.append -> 0x54351F5BB08D5BF7
                case 0x54351F5BB08D5BF7:
                {
                    string path = ReadBufferString(handleAddr);
                    if (!ViRam.TryGetValue(destAddr, out byte[] data))
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    try
                    {
                        using (var fs = File.Open(path, FileMode.Append, FileAccess.Write))
                        {
                            fs.Write(data, 0, data.Length);
                        }
                    }
                    catch
                    {
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    }
                    break;
                }
                // io.file.exists -> 0x31AA20F5AF2D74BF
                case 0x31AA20F5AF2D74BF:
                {
                    string path = ReadBufferString(handleAddr);
                    ulong exists = File.Exists(path) ? 1UL : 0UL;
                    WriteBufferU64(destAddr, exists);
                    break;
                }
                // io.file.length -> 0xB47329EB57B9802F
                case 0xB47329EB57B9802F:
                {
                    string path = ReadBufferString(handleAddr);
                    try
                    {
                        ulong len = (ulong)new FileInfo(path).Length;
                        WriteBufferU64(destAddr, len);
                    }
                    catch
                    {
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    }
                    break;
                }
                // u64.to_string -> 0x7118981E0DB56297
                case 0x7118981E0DB56297:
                {
                    if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    string text = BitConverter.ToUInt64(buff, 0).ToString(CultureInfo.InvariantCulture);
                    WriteStringLikeRust(destAddr, Encoding.UTF8.GetBytes(text));
                    break;
                }
                // i64.to_string -> 0xD6922AD279C2F043
                case 0xD6922AD279C2F043:
                {
                    if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    string text = BitConverter.ToInt64(buff, 0).ToString(CultureInfo.InvariantCulture);
                    WriteStringLikeRust(destAddr, Encoding.UTF8.GetBytes(text));
                    break;
                }
                // f64.to_string -> 0x5CB2D0209CD628A
                case 0x05CB2D0209CD628A:
                {
                    if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                        throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                    string text = BitConverter.ToDouble(buff, 0).ToString(CultureInfo.InvariantCulture);
                    WriteStringLikeRust(destAddr, Encoding.UTF8.GetBytes(text));
                    break;
                }
                default:
                    throw new RuntimeErrorException(RuntimeError.UnknownSyscallHash);
            }
        }

        private void syscall1_impl(ulong hash, InstrParam argP, InstrParam destP)
        {
            ulong argAddr = ResolveParam(argP);
            ulong destAddr = ResolveParam(destP);

            switch (hash)
            {
                // io.console.read_line -> 0x9B1285549BD07847
                case 0x9B1285549BD07847:
                {
                    string prompt = ReadBufferString(argAddr);
                    if (!string.IsNullOrEmpty(prompt))
                    {
                        Console.Write(prompt);
                        Console.Out.Flush();
                    }

                    string input = Console.ReadLine() ?? "";
                    WriteBufferBytes(destAddr, Encoding.UTF8.GetBytes(input));
                    break;
                }
                // u64.from_string -> 0x5F1C2A5DD8A72C34
                case 0x5F1C2A5DD8A72C34:
                {
                    string text = ReadBufferString(argAddr);
                    ulong value = ulong.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong parsed) ? parsed : 0UL;
                    WriteBufferU64(destAddr, value);
                    break;
                }
                // i64.from_string -> 0xDF1A83BF8FBBBA68
                case 0xDF1A83BF8FBBBA68:
                {
                    string text = ReadBufferString(argAddr);
                    long value = long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed) ? parsed : 0L;
                    WriteBufferBytes(destAddr, BitConverter.GetBytes(value));
                    break;
                }
                // f64.from_string -> 0x60D7394919E29C45
                case 0x60D7394919E29C45:
                {
                    string text = ReadBufferString(argAddr);
                    double value = double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? parsed : 0.0;
                    WriteBufferBytes(destAddr, BitConverter.GetBytes(value));
                    break;
                }
                default:
                    throw new RuntimeErrorException(RuntimeError.UnknownSyscallHash);
            }
        }

        private void WriteStringLikeRust(ulong addr, byte[] bytes)
        {
            if (!ViRam.TryGetValue(addr, out byte[] dest))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            if (dest.Length > bytes.Length)
            {
                Array.Copy(bytes, 0, dest, 0, bytes.Length);
                return;
            }

            ViRam[addr] = bytes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BedrockBytecodeEngine
{
    public class PVramEntry
    {
        public ulong OwnerId { get; set; }
        public byte[] Data { get; set; }

        public PVramEntry(ulong ownerId, byte[] data)
        {
            OwnerId = ownerId;
            Data = data;
        }
    }


    public class VM
    {
        public const int MaxStackSize = 0x10000;
        public const int MaxRegisters = 256;

        public ulong Id { get; }
        public ulong[] Reg { get; }
        public int Pc { get; set; }
        internal ulong nextHandle = 1;
        public Dictionary<ulong, byte[]> ViRam { get; } = new Dictionary<ulong, byte[]>();
        public Dictionary<ulong, int> LabelIndexs { get; } = new Dictionary<ulong, int>();
        public Stack<ulong> Stack { get; } = new Stack<ulong>();
        public Stack<int> CallStack { get; } = new Stack<int>();
        public GlobalState Global { get; }
        public IVMState State { get; set; }

        public VM(ulong id)
        {
            if (GlobalState.Instance.vm_ids.Contains(id))
                throw new BuildErrorException(BuildErrorKind.DuplicateVMId);
            Id = id;
            Reg = new ulong[MaxRegisters];
            Pc = 0;
            Global = GlobalState.Instance;
            Global.vm_ids.Add(id);
            State = new Running();
        }
        public VM(ulong id, GlobalState global)
        {
            if (global.vm_ids.Contains(id))
                throw new BuildErrorException(BuildErrorKind.DuplicateVMId);
            Id = id;
            Reg = new ulong[MaxRegisters];
            Pc = 0;
            Global = global;
            Global.vm_ids.Add(id);
            State = new Running();
        }

        public ulong Run(List<Instruction> instructions, int start)
        {
            Pc = start;
            int len = instructions.Count;

            while (Pc < len)
            {
                switch (State)
                {
                    case Running _:
                        break;
                    case Halted _:
                        return 0;
                    case Panicked panic:
                        Console.WriteLine($"VM Panicked <{panic.ErrorCode}>: {panic.ErrorMessage}");
                        throw new PanickedRuntimeException(panic.ErrorMessage, panic.ErrorCode);
                }
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
                    BMath.add(add.Reg, add.V2, Reg);
                    break;
                case Fadd fadd:
                    BMath.fadd(fadd.Reg, fadd.V2, Reg);
                    break;
                case Finv finv:
                    BMath.finv(finv.Reg, finv.V2, Reg);
                    break;
                case Sub sub:
                    BMath.sub(sub.Reg, sub.V2, Reg);
                    break;
                case Fsub fsub:
                    BMath.fsub(fsub.Reg, fsub.V2, Reg);
                    break;
                case Mult mult:
                    BMath.mult(mult.Reg, mult.V2, Reg);
                    break;
                case Fmult fmult:
                    BMath.fmult(fmult.Reg, fmult.V2, Reg);
                    break;
                case Div div:
                    BMath.div(div.Reg, div.V2, Reg);
                    break;
                case Fdiv fdiv:
                    BMath.fdiv(fdiv.Reg, fdiv.V2, Reg);
                    break;
                case Mod mod:
                    BMath.modulous(mod.Reg, mod.V2, Reg);
                    break;
                case Fmod fmod:
                    BMath.fmodulous(fmod.Reg, fmod.V2, Reg);
                    break;
                case Lshift lshift:
                    BMath.lshift(lshift.Reg, lshift.V2, Reg);
                    break;
                case Rshift rshift:
                    BMath.rshift(rshift.Reg, rshift.V2, Reg);
                    break;
                case Mov mov:
                    Memory.mov(mov.Reg, this, Reg, mov.V2);
                    break;
                case Sqrt sqrt:
                    BMath.sqrt(sqrt.Reg, sqrt.V2, Reg);
                    break;
                case Fsqrt fsqrt:
                    BMath.fsqrt(fsqrt.Reg, fsqrt.V2, Reg);
                    break;
                case Itof itof:
                    BMath.itof(itof.Reg, itof.V2, Reg);
                    break;
                case Ftoi ftoi:
                    BMath.ftoi(ftoi.Reg, ftoi.V2, Reg);
                    break;

                // Comparisons (Int)
                case Eq eq:
                    BMath.eq(eq.Reg, eq.V1, eq.V2, Reg);
                    break;
                case Neq neq:
                    BMath.neq(neq.Reg, neq.V1, neq.V2, Reg);
                    break;
                case Gt gt:
                    BMath.gt(gt.Reg, gt.V1, gt.V2, Reg);
                    break;
                case Lt lt:
                    BMath.lt(lt.Reg, lt.V1, lt.V2, Reg);
                    break;
                case Gte gte:
                    BMath.gte(gte.Reg, gte.V1, gte.V2, Reg);
                    break;
                case Lte lte:
                    BMath.lte(lte.Reg, lte.V1, lte.V2, Reg);
                    break;
                case And and:
                    BMath.and(and.Reg, and.V1, and.V2, Reg);
                    break;
                case Or or:
                    BMath.or(or.Reg, or.V1, or.V2, Reg);
                    break;
                case Xor xor:
                    BMath.xor(xor.Reg, xor.V1, xor.V2, Reg);
                    break;
                case Not not:
                    BMath.not(not.Reg, not.V1, Reg);
                    break;

                // Comparisons (Float)
                case Feq feq:
                    BMath.feq(feq.Reg, feq.V1, feq.V2, Reg);
                    break;
                case Fneq fneq:
                    BMath.fneq(fneq.Reg, fneq.V1, fneq.V2, Reg);
                    break;
                case Fgt fgt:
                    BMath.fgt(fgt.Reg, fgt.V1, fgt.V2, Reg);
                    break;
                case Flt flt:
                    BMath.flt(flt.Reg, flt.V1, flt.V2, Reg);
                    break;
                case Fgte fgte:
                    BMath.fgte(fgte.Reg, fgte.V1, fgte.V2, Reg);
                    break;
                case Flte flte:
                    BMath.flte(flte.Reg, flte.V1, flte.V2, Reg);
                    break;

                // Memory (Local)
                case Read read:
                    Memory.read(read.Reg, this, Reg, read.Addr, read.K);
                    break;
                case Write write:
                    Memory.write(write.Reg, this, Reg, write.Addr, write.K);
                    break;
                case Check check:
                    Memory.check(check.Reg, this, Reg, check.Addr, check.K);
                    break;
                case Alloc alloc:
                    Memory.alloc(alloc.Reg, this, Reg, alloc.Size);
                    break;
                case Realloc realloc:
                    Memory.realloc(realloc.Reg, this, Reg, realloc.Size);
                    break;
                case Free free:
                    Memory.free(free.Addr, this, Reg);
                    break;
                case Vlen vlen:
                    Memory.vlen(vlen.Reg, this, Reg, vlen.Loc);
                    break;
                case ReadW readw:
                    Memory.readw(readw.Reg, this, Reg, readw.Addr, readw.K);
                    break;
                case WriteW writew:
                    Memory.writew(writew.Reg, this, Reg, writew.Addr, writew.K);
                    break;

                // Memory (Persistent)
                case Palloc palloc:
                    Memory.palloc(palloc.Reg, this, Reg, palloc.Size);
                    break;
                case Prealloc prealloc:
                    Memory.prealloc(prealloc.Reg, this, Reg, prealloc.Size);
                    break;
                case Pfree pfree:
                    Memory.pfree(pfree.Addr, this, Reg);
                    break;
                case Pcheck pcheck:
                    Memory.pcheck(pcheck.Reg, this, Reg, pcheck.Addr, pcheck.K);
                    break;
                case Pread pread:
                    Memory.pread(pread.Reg, this, Reg, pread.Addr, pread.K);
                    break;
                case Pwrite pwrite:
                    Memory.pwrite(pwrite.Reg, this, Reg, pwrite.Addr, pwrite.K);
                    break;
                case PreadW preadw:
                    Memory.preadw(preadw.Reg, this, Reg, preadw.Addr, preadw.K);
                    break;
                case PwriteW pwritew:
                    Memory.pwritew(pwritew.Reg, this, Reg, pwritew.Addr, pwritew.K);
                    break;
                case Pvlen pvlen:
                    Memory.pvlen(pvlen.Reg, this, pvlen.Loc, Reg);
                    break;
                case Powner powner:
                    Memory.powner(powner.Reg, this, powner.Loc, Reg);
                    break;

                // Control Flow
                case Jmp jmp:
                    Control.jmp(jmp.JmpHash, this);
                    break;
                case Jif jif:
                    Control.jif(jif.Reg, jif.JmpHash, this);
                    break;
                case Jnif jnif:
                    Control.jnif(jnif.Reg, jnif.JmpHash, this);
                    break;
                case Call call:
                    Control.call(call.JmpHash, this);
                    break;
                case Ret ret:
                    Control.ret(this);
                    break;
                case Push push:
                    Control.push(push.Value, this);
                    break;
                case Pop pop:
                    Control.pop(pop.Reg, this);
                    break;

                // System
                case SyscallH syscallh:
                    syscallh_impl(syscallh.Hash, syscallh.Handle, syscallh.Dest);
                    break;
                case Syscall1 syscall1:
                    syscall1_impl(syscall1.Hash, syscall1.Arg, syscall1.Dest);
                    break;

                case Halt halt:
                    State = new Halted();
                    break;
                case Panic panic:
                    var msg_addr = ResolveParam(panic.Message);
                    string msg = "<no message provided>";
                    if (ViRam.TryGetValue(msg_addr, out byte[] buff))
                        msg = Encoding.UTF8.GetString(buff);
                    else if (msg_addr != 0)
                        msg = $"<invalid message address: {msg_addr}>";
                    State = new Panicked(panic.Code, msg);
                    break;

                case Assert assert:
                    {
                        ulong val = Reg[assert.Reg];
                        if (val == 0)
                        {
                            var msg_addr_ = ResolveParam(assert.Message);
                            string msg_ = "<no message provided>";
                            if (ViRam.TryGetValue(msg_addr_, out byte[] buff_))
                                msg_ = Encoding.UTF8.GetString(buff_);
                            else if (msg_addr_ != 0)
                                msg_ = $"<invalid message address: {msg_addr_}>";
                            State = new Panicked(1, $"Assertion Failed: {msg_}");
                        }
                        break;
                    }

                case Pass:
                    break;
            }
        }

        private ulong ResolveParam(InstrParam param)
        {
            return param.Type == InstrParamType.Register ? Reg[param.RegisterId] : param.Value;
        }

        public static ulong ResolveParam_reg(InstrParam param, ulong[] Reg)
        {
            return param.Type == InstrParamType.Register ? Reg[param.RegisterId] : param.Value;
        }


        // System Calls
        private string ReadBufferString(ulong addr)
        {
            if (!ViRam.TryGetValue(addr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);
            string str = Encoding.UTF8.GetString(buff).TrimEnd('\0');
            return str;
        }



        private void syscallh_impl(ulong hash, InstrParam handleP, InstrParam destP)
        {
            ulong handleAddr = ResolveParam(handleP);
            ulong destAddr = ResolveParam(destP);

            switch (hash)
            {
                // u64.to_string -> 0x7118981E0DB56297
                case 0x7118981E0DB56297:
                    {
                        if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                            throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                        string text = BitConverter.ToUInt64(buff, 0).ToString(CultureInfo.InvariantCulture);
                        Utils.WriteBufferBytes(this, destAddr, Encoding.UTF8.GetBytes(text));
                        break;
                    }
                // i64.to_string -> 0xD6922AD279C2F043
                case 0xD6922AD279C2F043:
                    {
                        if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                            throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                        string text = BitConverter.ToInt64(buff, 0).ToString(CultureInfo.InvariantCulture);
                        Utils.WriteBufferBytes(this, destAddr, Encoding.UTF8.GetBytes(text));
                        break;
                    }

                case 0x5CB2D0209CD628A:
                    {
                        if (!ViRam.TryGetValue(handleAddr, out byte[] buff))
                            throw new RuntimeErrorException(RuntimeError.InvalidAddress);
                        string text = BitConverter.ToDouble(buff, 0).ToString(CultureInfo.InvariantCulture);
                        Utils.WriteBufferBytes(this, destAddr, Encoding.UTF8.GetBytes(text));
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
                // u64.from_string -> 0x5F1C2A5DD8A72C34
                case 0x5F1C2A5DD8A72C34:
                    {
                        string text = ReadBufferString(argAddr);
                        ulong value = ulong.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong parsed) ? parsed : 0UL;
                        Utils.WriteBufferU64(this, destAddr, value);
                        break;
                    }
                // i64.from_string -> 0xDF1A83BF8FBBBA68
                case 0xDF1A83BF8FBBBA68:
                    {
                        string text = ReadBufferString(argAddr);
                        long value = long.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long parsed) ? parsed : 0L;
                        Utils.WriteBufferBytes(this, destAddr, BitConverter.GetBytes(value));
                        break;
                    }
                // f64.from_string -> 0x60D7394919E29C45
                case 0x60D7394919E29C45:
                    {
                        string text = ReadBufferString(argAddr);
                        double value = double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? parsed : 0.0;
                        Utils.WriteBufferBytes(this, destAddr, BitConverter.GetBytes(value));
                        break;
                    }
                default:
                    throw new RuntimeErrorException(RuntimeError.UnknownSyscallHash);
            }
        }


    }
}

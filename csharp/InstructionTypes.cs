using System;
using System.Collections.Generic;

namespace BedrockBytecodeEngine
{
    public enum InstrParamType
    {
        Register,
        Value
    }

    public struct InstrParam
    {
        public InstrParamType Type { get; set; }
        public byte RegisterId { get; set; }
        public ulong Value { get; set; }

        public static InstrParam Reg(byte regId)
        {
            return new InstrParam { Type = InstrParamType.Register, RegisterId = regId };
        }

        public static InstrParam Val(ulong value)
        {
            return new InstrParam { Type = InstrParamType.Value, Value = value };
        }
    }

    public abstract class Instruction { }

    // Memory (Local)
    public class Read : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Read(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Write : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Write(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Check : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Check(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Alloc : Instruction
    {
        public byte Reg { get; }
        public InstrParam Size { get; }

        public Alloc(byte reg, InstrParam size)
        {
            Reg = reg;
            Size = size;
        }
    }

    public class Realloc : Instruction
    {
        public byte Reg { get; }
        public InstrParam Size { get; }

        public Realloc(byte reg, InstrParam size)
        {
            Reg = reg;
            Size = size;
        }
    }

    public class Free : Instruction
    {
        public InstrParam Addr { get; }

        public Free(InstrParam addr)
        {
            Addr = addr;
        }
    }

    public class Vlen : Instruction
    {
        public byte Reg { get; }
        public InstrParam Loc { get; }

        public Vlen(byte reg, InstrParam loc)
        {
            Reg = reg;
            Loc = loc;
        }
    }

    public class ReadW : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public ReadW(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class WriteW : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public WriteW(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    // Memory (Global Persistent)
    public class Palloc : Instruction
    {
        public byte Reg { get; }
        public InstrParam Size { get; }

        public Palloc(byte reg, InstrParam size)
        {
            Reg = reg;
            Size = size;
        }
    }

    public class Prealloc : Instruction
    {
        public byte Reg { get; }
        public InstrParam Size { get; }

        public Prealloc(byte reg, InstrParam size)
        {
            Reg = reg;
            Size = size;
        }
    }

    public class Pfree : Instruction
    {
        public InstrParam Addr { get; }

        public Pfree(InstrParam addr)
        {
            Addr = addr;
        }
    }

    public class Pcheck : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Pcheck(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Pread : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Pread(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Pwrite : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public Pwrite(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class PreadW : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public PreadW(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class PwriteW : Instruction
    {
        public byte Reg { get; }
        public InstrParam Addr { get; }
        public InstrParam K { get; }

        public PwriteW(byte reg, InstrParam addr, InstrParam k)
        {
            Reg = reg;
            Addr = addr;
            K = k;
        }
    }

    public class Pvlen : Instruction
    {
        public byte Reg { get; }
        public InstrParam Loc { get; }

        public Pvlen(byte reg, InstrParam loc)
        {
            Reg = reg;
            Loc = loc;
        }
    }

    public class Powner : Instruction
    {
        public byte Reg { get; }
        public InstrParam Loc { get; }

        public Powner(byte reg, InstrParam loc)
        {
            Reg = reg;
            Loc = loc;
        }
    }

    // Integer Math
    public class Add : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Add(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Sub : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Sub(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Mult : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Mult(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Div : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Div(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Mod : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Mod(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Lshift : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Lshift(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Rshift : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Rshift(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Sqrt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Sqrt(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    // Float Math
    public class Fadd : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fadd(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Fsub : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fsub(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Fmult : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fmult(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Fdiv : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fdiv(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Fmod : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fmod(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Fsqrt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Fsqrt(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Finv : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Finv(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    // Logic & Comparison (Int)
    public class Eq : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Eq(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Neq : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Neq(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Lt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Lt(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Gt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Gt(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Lte : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Lte(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Gte : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Gte(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class And : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public And(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Or : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Or(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Xor : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Xor(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Not : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }

        public Not(byte reg, InstrParam v1)
        {
            Reg = reg;
            V1 = v1;
        }
    }

    // Logic & Comparison (Float)
    public class Feq : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Feq(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Fneq : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Fneq(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Flt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Flt(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Fgt : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Fgt(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Flte : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Flte(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    public class Fgte : Instruction
    {
        public byte Reg { get; }
        public InstrParam V1 { get; }
        public InstrParam V2 { get; }

        public Fgte(byte reg, InstrParam v1, InstrParam v2)
        {
            Reg = reg;
            V1 = v1;
            V2 = v2;
        }
    }

    // Control Flow
    public class Jmp : Instruction
    {
        public ulong JmpHash { get; }

        public Jmp(ulong jmpHash)
        {
            JmpHash = jmpHash;
        }
    }

    public class Jif : Instruction
    {
        public byte Reg { get; }
        public ulong JmpHash { get; }

        public Jif(byte reg, ulong jmpHash)
        {
            Reg = reg;
            JmpHash = jmpHash;
        }
    }

    public class Jnif : Instruction
    {
        public byte Reg { get; }
        public ulong JmpHash { get; }

        public Jnif(byte reg, ulong jmpHash)
        {
            Reg = reg;
            JmpHash = jmpHash;
        }
    }

    public class Call : Instruction
    {
        public ulong JmpHash { get; }

        public Call(ulong jmpHash)
        {
            JmpHash = jmpHash;
        }
    }

    public class Ret : Instruction { }

    public class Ecall : Instruction
    {
        public ulong V1 { get; }
        public ulong V2 { get; }

        public Ecall(ulong v1, ulong v2)
        {
            V1 = v1;
            V2 = v2;
        }
    }

    public class Push : Instruction
    {
        public InstrParam Value { get; }

        public Push(InstrParam value)
        {
            Value = value;
        }
    }

    public class Pop : Instruction
    {
        public byte Reg { get; }

        public Pop(byte reg)
        {
            Reg = reg;
        }
    }

    // Data & System
    public class Mov : Instruction
    {
        public byte Reg { get; }
        public InstrParam V2 { get; }

        public Mov(byte reg, InstrParam v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Itof : Instruction
    {
        public byte Reg { get; }
        public byte V2 { get; }

        public Itof(byte reg, byte v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class Ftoi : Instruction
    {
        public byte Reg { get; }
        public byte V2 { get; }

        public Ftoi(byte reg, byte v2)
        {
            Reg = reg;
            V2 = v2;
        }
    }

    public class SyscallH : Instruction
    {
        public ulong Hash { get; }
        public InstrParam Handle { get; }
        public InstrParam Dest { get; }

        public SyscallH(ulong hash, InstrParam handle, InstrParam dest)
        {
            Hash = hash;
            Handle = handle;
            Dest = dest;
        }
    }

    public class Syscall1 : Instruction
    {
        public ulong Hash { get; }
        public InstrParam Arg { get; }
        public InstrParam Dest { get; }

        public Syscall1(ulong hash, InstrParam arg, InstrParam dest)
        {
            Hash = hash;
            Arg = arg;
            Dest = dest;
        }
    }

    public class Pass : Instruction { }
}

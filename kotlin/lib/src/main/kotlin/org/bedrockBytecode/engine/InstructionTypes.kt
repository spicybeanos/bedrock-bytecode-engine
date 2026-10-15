package org.bedrockBytecode.engine

public enum class InstrParamType{
    Register,
    Value
}

public enum class InstructionNumbers{
    Pass,
    // Memory (Local)
    Read,
    Write,
    Check,
    Alloc,
    Realloc,
    Free,
    Vlen,
    ReadW,
    WriteW,

    // Memory (Global Persistent)
    Palloc,
    Prealloc,
    Pfree,
    Pcheck,
    Pread,
    Pwrite,
    PreadW,
    PwriteW,
    Pvlen,
    Powner,

    // Integer Math
    Add,
    Sub,
    Mult,
    Div,
    Mod,
    Lshift,
    Rshift,
    Sqrt,

    // Float Math
    Fadd,
    Fsub,
    Fmult,
    Fdiv,
    Fmod,
    Fsqrt,
    Finv,

    // Logic & Comparison (Int)
    Eq,
    Neq,
    Lt,
    Gt,
    Lte,
    Gte,
    And,
    Or,
    Xor,
    Not,

    // Logic & Comparison (Float)
    Feq,
    Fneq,
    Flt,
    Fgt,
    Flte,
    Fgte,

    // Control Flow
    Jmp,
    Jif,
    Jnif,
    Call,
    Ret,
    Ecall,
    Push,
    Pop,

    // Data & System
    Mov,
    Itof,
    Ftoi,
    SyscallH,
    Syscall1,
    Import,

    Halt,
    Panic,
    Assert,
}

internal class InstrParam (public val Type : InstrParamType ,public var  RegisterId:UByte,public var  Value:ULong){
       

    public fun  Reg( regId:UByte) : InstrParam 
        {
            return InstrParam (Type = InstrParamType.Register, RegisterId = regId,Value = 0UL)
        }

        public fun Val( value:ULong) : InstrParam
        {
            return InstrParam ( Type = InstrParamType.Value, Value = value , RegisterId = 0u)
        }
}

public interface Instruction { public fun getInstructionNumber():Int }



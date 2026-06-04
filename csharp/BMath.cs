
using System;

namespace BedrockBytecodeEngine
{
    public class BMath
    {
        // Math Operations
        internal static void add(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] += val;
        }

        internal static void fadd(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) + val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void finv(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = 1.0 / val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void sub(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] -= val;
        }

        internal static void fsub(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) - val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void mult(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] *= val;
        }

        internal static void fmult(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) * val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void div(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] /= val;
        }

        internal static void fdiv(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) / val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void modulous(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] %= val;
        }

        internal static void fmodulous(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = BitConverter.Int64BitsToDouble((long)Reg[r]) % val;
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        internal static void lshift(byte r, InstrParam v2, ulong[] Reg)
        {
            int val = (int)VM.ResolveParam_reg(v2, Reg);
            Reg[r] <<= val;
        }

        internal static void rshift(byte r, InstrParam v2, ulong[] Reg)
        {
            int val = (int)VM.ResolveParam_reg(v2, Reg);
            Reg[r] >>= val;
        }

        internal static void itof(byte r, byte v2, ulong[] Reg)
        {
            double val = (double)Reg[v2];
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(val);
        }

        internal static void ftoi(byte r, byte v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)Reg[v2]);
            Reg[r] = (ulong)(long)val;
        }

        internal static void sqrt(byte r, InstrParam v2, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = (ulong)Math.Sqrt(val);
        }

        internal static void fsqrt(byte r, InstrParam v2, ulong[] Reg)
        {
            double val = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            double res = Math.Sqrt(val);
            Reg[r] = (ulong)BitConverter.DoubleToInt64Bits(res);
        }

        // binary math

        internal static void eq(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 == val2 ? 1UL : 0UL;
        }

        internal static void neq(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 != val2 ? 1UL : 0UL;
        }

        internal static void lt(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 < val2 ? 1UL : 0UL;
        }

        internal static void gt(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 > val2 ? 1UL : 0UL;
        }
        internal static void lte(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 <= val2 ? 1UL : 0UL;
        }
        

        internal static void gte(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 >= val2 ? 1UL : 0UL;
        }

        internal static void and(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 & val2;
        }

        internal static void or(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 | val2;
        }

        internal static void xor(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            ulong val1 = VM.ResolveParam_reg(v1, Reg);
            ulong val2 = VM.ResolveParam_reg(v2, Reg);
            Reg[r] = val1 ^ val2;
        }

        internal static void not(byte r, InstrParam v1, ulong[] Reg)
        {
            ulong val = VM.ResolveParam_reg(v1, Reg);
            Reg[r] = ~val;
        }

        // Comparison Operations (Float)
        internal static void feq(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 == val2 ? 1UL : 0UL;
        }

        internal static void fneq(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 != val2 ? 1UL : 0UL;
        }

        internal static void flt(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 < val2 ? 1UL : 0UL;
        }

        internal static void fgt(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 > val2 ? 1UL : 0UL;
        }

        internal static void flte(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 <= val2 ? 1UL : 0UL;
        }

        internal static void fgte(byte r, InstrParam v1, InstrParam v2, ulong[] Reg)
        {
            double val1 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v1, Reg));
            double val2 = BitConverter.Int64BitsToDouble((long)VM.ResolveParam_reg(v2, Reg));
            Reg[r] = val1 >= val2 ? 1UL : 0UL;
        }
    }
}
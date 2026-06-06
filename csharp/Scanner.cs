using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BedrockBytecodeEngine
{
    public enum PreProcessType
    {
        MemString,
        MemBlob,
        Label
    }

    public class PreProcess
    {
        public PreProcessType Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public List<byte> BlobValues { get; set; }
        public ulong LabelHash { get; set; }
        public int Line { get; set; }
    }

    public class Scanner
    {
        public static (VM, List<Instruction>) ConstructFromText(
            ulong id,
            string text
        )
        {
            RefreshableString workingText = new RefreshableString(text);
            VM vm = new VM(id);
            List<Instruction> instructions = new List<Instruction>();
            List<PreProcess> preProccssCmds = new List<PreProcess>();

            int index = 0;
            int len = text.Length;
            string cur = "";
            bool preProcess = false;
            bool lineStart = true;
            bool inQuotes = false;
            int lineNumber = 1;
            List<string> strLine = new List<string>();
            List<string> strPreproc = new List<string>();

            while (index < len)
            {
                char c = text[index];

                switch (c)
                {
                    case ' ':
                    case '\t':
                        if (!inQuotes)
                        {
                            PushCurrentToken(cur, preProcess, strLine, strPreproc);
                            cur = "";
                            lineStart = false;
                        }
                        else
                        {
                            cur += c;
                        }
                        break;
                    case '\n':
                        PushCurrentToken(cur, preProcess, strLine, strPreproc);
                        cur = "";
                        FinalizeLine(
                            preProcess,
                            strLine,
                            strPreproc,
                            preProccssCmds,
                            instructions,
                            lineNumber
                        );
                        preProcess = false;
                        lineStart = true;
                        inQuotes = false;
                        lineNumber++;
                        break;
                    case '\r':
                        PushCurrentToken(cur, preProcess, strLine, strPreproc);
                        cur = "";
                        FinalizeLine(
                            preProcess,
                            strLine,
                            strPreproc,
                            preProccssCmds,
                            instructions,
                            lineNumber
                        );
                        preProcess = false;
                        lineStart = true;
                        inQuotes = false;

                        // Treat CRLF as a single line ending
                        if (index + 1 < len && text[index + 1] == '\n')
                        {
                            index++;
                        }
                        lineNumber++;
                        break;
                    case ':':
                        if (lineStart)
                        {
                            preProcess = true;
                        }
                        else
                        {
                            cur += c;
                        }
                        lineStart = false;
                        break;
                    case '"':
                        inQuotes = !inQuotes;
                        cur += c;
                        lineStart = false;
                        break;
                    default:
                        cur += c;
                        lineStart = false;
                        break;
                }

                index++;
            }

            // Handle any remaining content
            PushCurrentToken(cur, preProcess, strLine, strPreproc);
            FinalizeLine(preProcess, strLine, strPreproc, preProccssCmds, instructions, lineNumber);

            ApplyPreprocessCmds(vm, preProccssCmds);

            return (vm, instructions);
        }

        private static void PushCurrentToken(
            string cur,
            bool preProcess,
            List<string> strLine,
            List<string> strPreproc
        )
        {
            if (string.IsNullOrEmpty(cur))
                return;

            if (preProcess)
            {
                strPreproc.Add(cur);
            }
            else
            {
                strLine.Add(cur);
            }
        }

        private static void FinalizeLine(
            bool preProcess,
            List<string> strLine,
            List<string> strPreproc,
            List<PreProcess> preProccssCmds,
            List<Instruction> instructions,
            int lineNumber
        )
        {
            if (preProcess)
            {
                if (strPreproc.Count > 0)
                {
                    ParsePreprocessLine(strPreproc, preProccssCmds, lineNumber);
                }
                instructions.Add(new Pass());
            }
            else if (strLine.Count > 0)
            {
                instructions.Add(InstructionFromString(strLine));
            }

            strLine.Clear();
            strPreproc.Clear();
        }

        private static void ParsePreprocessLine(
            List<string> strPreproc,
            List<PreProcess> preProccssCmds,
            int lineNumber
        )
        {
            if (strPreproc.Count == 0)
                return;

            string cmd = strPreproc[0];

            switch (cmd)
            {
                case "string":
                    if (strPreproc.Count < 3)
                        throw new BuildErrorException(
                            BuildErrorKind.InvalidOrMissingParameter,
                            lineNumber
                        );

                    {
                        string name = strPreproc[1];
                        string raw = string.Join(" ", strPreproc.Skip(2));
                        if (string.IsNullOrEmpty(raw))
                            throw new BuildErrorException(
                                BuildErrorKind.InvalidOrMissingParameter,
                                lineNumber
                            );

                        string value = raw;
                        if (raw.StartsWith("\"") && raw.EndsWith("\""))
                        {
                            value = raw.Substring(1, raw.Length - 2);
                        }

                        preProccssCmds.Add(
                            new PreProcess
                            {
                                Type = PreProcessType.MemString,
                                Name = name,
                                Value = value,
                                Line = lineNumber
                            }
                        );
                    }
                    break;

                case "buff":
                    if (strPreproc.Count < 3)
                        throw new BuildErrorException(
                            BuildErrorKind.InvalidOrMissingParameter,
                            lineNumber
                        );

                    {
                        string name = strPreproc[1];
                        string values = string.Join(" ", strPreproc.Skip(2));
                        if (string.IsNullOrEmpty(values))
                            throw new BuildErrorException(
                                BuildErrorKind.InvalidOrMissingParameter,
                                lineNumber
                            );

                        preProccssCmds.Add(
                            new PreProcess
                            {
                                Type = PreProcessType.MemBlob,
                                Name = name,
                                Value = values,
                                Line = lineNumber
                            }
                        );
                    }
                    break;

                case "label":
                    if (strPreproc.Count < 2)
                        throw new BuildErrorException(
                            BuildErrorKind.InvalidOrMissingParameter,
                            lineNumber
                        );

                    {
                        string name = strPreproc[1];
                        ulong hash = Utils.HashStringVm(name);
                        preProccssCmds.Add(
                            new PreProcess
                            {
                                Type = PreProcessType.Label,
                                LabelHash = hash,
                                Line = lineNumber
                            }
                        );
                    }
                    break;

                default:
                    throw new BuildErrorException(BuildErrorKind.InvalidParameter, lineNumber);
            }
        }

        private static void ApplyPreprocessCmds(VM vm, List<PreProcess> preProccssCmds)
        {
            foreach (PreProcess cmd in preProccssCmds)
            {
                switch (cmd.Type)
                {
                    case PreProcessType.MemString:
                        vm.ViRam[Utils.HashStringVm(cmd.Name)] = System.Text.Encoding.UTF8.GetBytes(
                            cmd.Value
                        );
                        break;

                    case PreProcessType.MemBlob:

                        {
                            List<byte> buff = new List<byte>();
                            foreach (
                                string token in cmd.Value.Split(
                                    new[] { ' ', '\t' },
                                    System.StringSplitOptions.RemoveEmptyEntries
                                )
                            )
                            {
                                buff.Add(ParseBlobNumber(token, cmd.Line));
                            }
                            vm.ViRam[Utils.HashStringVm(cmd.Name)] = buff.ToArray();
                        }
                        break;

                    case PreProcessType.Label:
                        vm.LabelIndexs[cmd.LabelHash] = cmd.Line;
                        break;
                }
            }
        }

        private static byte ParseBlobNumber(string token, int lineNumber)
        {
            string t = token.Trim();
            if (string.IsNullOrEmpty(t))
                throw new BuildErrorException(BuildErrorKind.InvalidParameter, lineNumber);

            // Try hex format
            if (t.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (
                    byte.TryParse(
                        t.Substring(2),
                        System.Globalization.NumberStyles.HexNumber,
                        null,
                        out byte hexVal
                    )
                )
                {
                    return hexVal;
                }
                throw new BuildErrorException(BuildErrorKind.InvalidParameter, lineNumber);
            }

            // Try float format
            if (t.Contains('.'))
            {
                if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double floatVal))
                {
                    if (floatVal >= 0 && floatVal <= 255)
                        return (byte)floatVal;
                }
                throw new BuildErrorException(BuildErrorKind.InvalidParameter, lineNumber);
            }

            // Try integer format
            if (long.TryParse(t, out long intVal))
            {
                if (intVal >= 0 && intVal <= 255)
                    return (byte)intVal;
            }

            throw new BuildErrorException(BuildErrorKind.InvalidParameter, lineNumber);
        }

        private static Instruction InstructionFromString(List<string> tokens)
        {
            if (tokens.Count == 0)
                return new Pass();

            string cmd = tokens[0];

            return cmd switch
            {
                "read"
                    => new Read(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "write"
                    => new Write(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "check"
                    => new Check(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "alloc"
                    => new Alloc(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "realloc"
                    => new Realloc(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "free" => new Free(ResolveParam(tokens.ElementAtOrDefault(1) ?? "")),
                "vlen"
                    => new Vlen(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "readw"
                or "oread"
                    => new ReadW(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "writew"
                or "owrite"
                    => new WriteW(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),

                "palloc"
                    => new Palloc(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "prealloc"
                    => new Prealloc(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "pfree" => new Pfree(ResolveParam(tokens.ElementAtOrDefault(1) ?? "")),
                "pcheck"
                    => new Pcheck(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "pread"
                    => new Pread(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "pwrite"
                    => new Pwrite(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "preadw"
                    => new PreadW(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "pwritew"
                    => new PwriteW(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "pvlen"
                    => new Pvlen(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "powner"
                    => new Powner(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),

                "add"
                    => new Add(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "sub"
                    => new Sub(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "mult"
                    => new Mult(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "div"
                    => new Div(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "mod"
                    => new Mod(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "lshift"
                    => new Lshift(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "rshift"
                    => new Rshift(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "sqrt"
                    => new Sqrt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),

                "fadd"
                    => new Fadd(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "fsub"
                    => new Fsub(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "fmult"
                    => new Fmult(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "fdiv"
                    => new Fdiv(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "fmod"
                    => new Fmod(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "fsqrt"
                    => new Fsqrt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "finv"
                    => new Finv(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),

                "eq"
                    => new Eq(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "neq"
                    => new Neq(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "lt"
                    => new Lt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "gt"
                    => new Gt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "lte"
                    => new Lte(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "gte"
                    => new Gte(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "and"
                    => new And(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "or"
                    => new Or(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "xor"
                    => new Xor(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "not"
                    => new Not(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),

                "feq"
                    => new Feq(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "fneq"
                    => new Fneq(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "flt"
                    => new Flt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "fgt"
                    => new Fgt(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "flte"
                    => new Flte(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "fgte"
                    => new Fgte(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),

                "jmp" => new Jmp(ResolveParamU64OrHash(tokens.ElementAtOrDefault(1) ?? "")),
                "jif"
                    => new Jif(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "jnif"
                    => new Jnif(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "call" => new Call(ResolveParamU64OrHash(tokens.ElementAtOrDefault(1) ?? "")),
                "ret" => new Ret(),
                "ecall"
                    => new Ecall(
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "push" => new Push(ResolveParam(tokens.ElementAtOrDefault(1) ?? "")),
                "pop" => new Pop(ResolveParamReg(tokens.ElementAtOrDefault(1) ?? "")),

                "mov"
                    => new Mov(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "itof"
                    => new Itof(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParamReg(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "ftoi"
                    => new Ftoi(
                        ResolveParamReg(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParamReg(tokens.ElementAtOrDefault(2) ?? "")
                    ),
                "syscallh"
                    => new SyscallH(
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),
                "syscall1"
                    => new Syscall1(
                        ResolveParamU64OrHash(tokens.ElementAtOrDefault(1) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(2) ?? ""),
                        ResolveParam(tokens.ElementAtOrDefault(3) ?? "")
                    ),

                "" => new Pass(),
                _ => throw new BuildErrorException(BuildErrorKind.UnknownInstruction)
            };
        }

        private static ulong ResolveParamU64OrHash(string param)
        {
            string p = param.Trim();
            if (string.IsNullOrEmpty(p))
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);

            if (p.StartsWith(":"))
                return Utils.HashStringVm(p.Substring(1));

            if (p.StartsWith("."))
                return Utils.HashStringVm(p.Substring(1));

            if (p.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                string hex = p.Substring(2);
                if (
                    ulong.TryParse(
                        hex,
                        System.Globalization.NumberStyles.HexNumber,
                        null,
                        out ulong val
                    )
                )
                    return val;
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);
            }

            if (ulong.TryParse(p, out ulong intVal))
                return intVal;

            throw new BuildErrorException(BuildErrorKind.InvalidParameter);
        }

        private static InstrParam ResolveParam(string param)
        {
            string p = param.Trim();
            if (string.IsNullOrEmpty(p))
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);

            if (p.StartsWith("."))
                return InstrParam.Val(Utils.HashStringVm(p.Substring(1)));

            if (p.StartsWith("r"))
            {
                string regStr = p.Substring(1);
                if (byte.TryParse(regStr, out byte regId))
                    return InstrParam.Reg(regId);
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);
            }

            if (p.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                string hex = p.Substring(2);
                if (
                    ulong.TryParse(
                        hex,
                        System.Globalization.NumberStyles.HexNumber,
                        null,
                        out ulong val
                    )
                )
                    return InstrParam.Val(val);
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);
            }

            if (p.Contains("."))
            {
                if (double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out double floatVal))
                    return InstrParam.Val((ulong)BitConverter.DoubleToInt64Bits(floatVal));
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);
            }

            if (long.TryParse(p, out long intVal))
                return InstrParam.Val((ulong)intVal);

            if (ulong.TryParse(p, out ulong uintVal))
                return InstrParam.Val(uintVal);

            throw new BuildErrorException(BuildErrorKind.InvalidParameter);
        }

        private static byte ResolveParamReg(string param)
        {
            string p = param.Trim();
            if (string.IsNullOrEmpty(p))
                throw new BuildErrorException(BuildErrorKind.InvalidParameter);

            if (p.StartsWith("r"))
            {
                string regStr = p.Substring(1);
                if (byte.TryParse(regStr, out byte regId))
                    return regId;
            }

            throw new BuildErrorException(BuildErrorKind.InvalidParameter);
        }
    }

    // Helper class to support string operations
    internal class RefreshableString
    {
        public string Value { get; set; }

        public RefreshableString(string value)
        {
            Value = value;
        }
    }
}

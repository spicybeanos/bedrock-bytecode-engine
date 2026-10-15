using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BedrockBytecodeEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                RunMathTest();
                RunMemoryTest();
                PrintHashes();
                return;
            }

            string path = args[0];

            // Check file extension
            string ext = Path.GetExtension(path);
            if (!ext.Equals(".bdt", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("extension incorrect");
                return;
            }

            string content;
            try
            {
                content = File.ReadAllText(path);
            }
            catch
            {
                Console.WriteLine("failed to read file");
                return;
            }

            VM vm;
            List<Instruction> instructions;
            try
            {
                (vm, instructions) = Scanner.ConstructFromText(1, content);
            }
            catch (BuildErrorException ex)
            {
                if (ex.LineNumber.HasValue)
                {
                    Console.WriteLine($"build error on line {ex.LineNumber.Value}: {BuildErrorName(ex.Kind)}");
                }
                else
                {
                    Console.WriteLine($"build error: {BuildErrorName(ex.Kind)}");
                }
                return;
            }

            try
            {
                vm.Run(instructions, 0);
            }
            catch (RuntimeErrorException ex)
            {
                Console.WriteLine($"runtime error: {RuntimeErrorName(ex)}");
            }

            DumpFirst10Registers(vm);
            DumpViRam(vm);
            DumpPVram(vm);
        }

        static void DumpFirst10Registers(VM vm)
        {
            Console.WriteLine("register dump (r0-r9):");
            for (int i = 0; i < 10; i++)
            {
                ulong value = vm.Reg[i];
                Console.WriteLine($"r{i} = {value} (0x{value:X})");
            }
        }

        static void DumpViRam(VM vm)
        {
            Console.WriteLine("vi_ram memdump:");
            if (vm.ViRam.Count == 0)
            {
                Console.WriteLine("<empty>");
                return;
            }

            var addrs = vm.ViRam.Keys.OrderBy(x => x).ToList();

            foreach (ulong addr in addrs)
            {
                byte[] bytes = vm.ViRam[addr];
                Console.Write($"addr {addr} (0x{addr:X}) len {bytes.Length}: ");
                for (int idx = 0; idx < bytes.Length; idx++)
                {
                    if (idx > 0)
                        Console.Write(" ");
                    Console.Write($"{bytes[idx]:X2}");
                }
                Console.WriteLine();
            }
        }

        static void DumpPVram(VM vm)
        {
            Console.WriteLine("p_vram memdump:");
            if (GlobalState.Instance.PVram.Count == 0)
            {
                Console.WriteLine("<empty>");
                return;
            }

            var addrs = GlobalState.Instance.PVram.Keys.OrderBy(x => x).ToList();

            foreach (ulong addr in addrs)
            {
                PVramEntry entry = GlobalState.Instance.PVram[addr];
                Console.Write(
                    $"addr {addr} (0x{addr:X}) owner {entry.OwnerId} len {entry.Data.Length}: "
                );
                for (int idx = 0; idx < entry.Data.Length; idx++)
                {
                    if (idx > 0)
                        Console.Write(" ");
                    Console.Write($"{entry.Data[idx]:X2}");
                }
                Console.WriteLine();
            }
        }

        static void RunMathTest()
        {
            List<Instruction> instr = new List<Instruction>
            {
                new Mov(1, InstrParam.Val(20)),
                new Mov(2, InstrParam.Val(20)),
                new Mult(1, InstrParam.Reg(2))
            };

            GlobalState.NewShared();
            VM vm = new VM(1);
            vm.Run(instr, 0);

            Console.WriteLine($"value of r1 = {vm.Reg[1]}");
        }

        static void PrintHashes()
        {
            string[] fxns = new[]
            {
                "io.console.print",
                "io.console.read_line",
                "io.file.read",
                "io.file.write",
                "io.file.append",
                "io.file.exists",
                "io.file.length",
                "u64.from_string",
                "i64.from_string",
                "f64.from_string",
                "u64.to_string",
                "i64.to_string",
                "f64.to_string",
                "native.crypto",
                "native.crypto.hash.sha1",
                "native.crypto.hash.sha256",
                "native.crypto.aes",
                "native.crypto.aes.decrypt",
                "native.crypto.aes.encrypt",
                "native.crypto.rsa",
                "native.crypto.rsa.keygen",
                "native.crypto.rsa.encrypt",
                "native.crypto.rsa.decrypt",
                
            };

            Console.WriteLine("hashes");
            string fcont = "string,hash,hash_16\n";
            foreach (string f in fxns)
            {
                ulong h = Utils.HashStringVm(f);
                Console.WriteLine($"{f} \t->\t {h} \t({h:X})");
                fcont += $"{f},{h},{h:X}\n";
            }
            File.WriteAllText("string hash result.csv",fcont);
        }

        static void RunMemoryTest()
        {
            List<Instruction> instr = new List<Instruction>
            {
                new Mov(1, InstrParam.Val(400)),
                new Alloc(3, InstrParam.Val(8)),
                new WriteW(1, InstrParam.Reg(3), InstrParam.Val(0)),
                new ReadW(4, InstrParam.Reg(3), InstrParam.Val(0))
            };

            GlobalState.NewShared();
            VM vm = new VM(1);
            vm.Run(instr, 0);

            Console.WriteLine($"value written: {vm.Reg[1]}");
            Console.WriteLine($"value read: {vm.Reg[4]}");
            Console.WriteLine($"Memory Test passed: {vm.Reg[1] == vm.Reg[4]}");
        }

        static string BuildErrorName(BuildErrorKind kind)
        {
            return kind switch
            {
                BuildErrorKind.UnknownInstruction => "unknown instruction",
                BuildErrorKind.InvalidParameter => "invalid parameter",
                BuildErrorKind.InvalidOrMissingParameter => "invalid or missing parameter",
                BuildErrorKind.FileSizeLT0 => "invalid file size",
                _ => "unknown error"
            };
        }

        static string RuntimeErrorName(RuntimeErrorException error)
        {
            return error.Error switch
            {
                RuntimeError.UnknownSyscallHash => "unknown syscall hash",
                RuntimeError.InvalidAddress => "invalid address",
                RuntimeError.OutOfBounds => "out of bounds",
                RuntimeError.NotOwnerOfBuffer => "not owner of buffer",
                RuntimeError.InvalidJumpHash => "invalid jump label hash",
                RuntimeError.EmptyCallStack => "empty call stack",
                RuntimeError.EmptyStack => "empty stack",
                RuntimeError.CallStackOverflow => "call stack overflow",
                RuntimeError.StackOverflow => "stack overflow",
                RuntimeError.Panicked => $"panicked <{(error as PanickedRuntimeException).ErrorCode}>: {(error as PanickedRuntimeException).ErrorMessage}",
                _ => "unknown error"
            };
        }
    }
}

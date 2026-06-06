


using System.Collections.Generic;

namespace BedrockBytecodeEngine
{
    public class SyscallFunction
    {
        public string FullName { get; private set;}
        public ulong Hash { get; private set;}
        public SyscallFunction(string name)
        {
            FullName = name;
            Hash = Utils.HashStringVm(name);
        }
        public virtual byte[] run(byte[] args)
        {
            return [];
        }
    }
    public class Syscall
    {
        public static Dictionary<ulong,SyscallFunction> Syscalls {get;private set;} = new Dictionary<ulong, SyscallFunction>();
        internal static void syscallh(VM vm,ulong hash, InstrParam handleP, InstrParam destP)
        {
            ulong handleAddr = VM.ResolveParam_reg(handleP,vm.Reg);
            ulong destAddr = VM.ResolveParam_reg(destP,vm.Reg);

            if(!Syscalls.ContainsKey(hash))
                throw new RuntimeErrorException(RuntimeError.UnknownSyscallHash);

            if (!vm.ViRam.TryGetValue(handleAddr, out byte[] buff))
                throw new RuntimeErrorException(RuntimeError.InvalidAddress);

            byte[] result = Syscalls[hash].run(buff);

            Utils.WriteBufferBytes(vm, destAddr, result);
        }
    }
}
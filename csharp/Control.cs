


namespace BedrockBytecodeEngine
{
    public class Control
    {
        internal static void jmp(ulong jmpHash, VM vm)
        {
            if (!vm.LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            vm.Pc = pc;
        }

        internal static void jif(byte r, ulong jmpHash, VM vm)
        {
            if (!vm.LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (vm.Reg[r] == 1)
                vm.Pc = pc;
        }

        internal static void jnif(byte r, ulong jmpHash, VM vm)
        {
            if (!vm.LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (vm.Reg[r] != 1)
                vm.Pc = pc;
        }

        internal static void call(ulong jmpHash, VM vm)
        {
            if (!vm.LabelIndexs.TryGetValue(jmpHash, out int pc))
                throw new RuntimeErrorException(RuntimeError.InvalidJumpHash);
            if (vm.CallStack.Count >= VM.MaxStackSize)
                throw new RuntimeErrorException(RuntimeError.CallStackOverflow);
            vm.CallStack.Push(vm.Pc);
            vm.Pc = pc;
        }

        internal static void ret(VM vm)
        {
            if (vm.CallStack.Count == 0)
                throw new RuntimeErrorException(RuntimeError.EmptyCallStack);
            vm.Pc = vm.CallStack.Peek();
            vm.CallStack.Pop();
        }

        internal static void push(InstrParam valueP, VM vm)
        {
            ulong value = VM.ResolveParam_reg(valueP, vm.Reg);
            if (vm.Stack.Count >= VM.MaxStackSize)
                throw new RuntimeErrorException(RuntimeError.StackOverflow);
            vm.Stack.Push(value);
        }

        internal static void pop(byte r, VM vm)
        {
            if (vm.Stack.Count == 0)
                throw new RuntimeErrorException(RuntimeError.EmptyStack);
            vm.Reg[r] = vm.Stack.Pop();
        }
    }
}
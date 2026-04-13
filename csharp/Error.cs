using System;

namespace BedrockBytecodeEngine
{
    public enum BuildErrorKind
    {
        UnknownInstruction,
        InvalidParameter,
        InvalidOrMissingParameter,
        FileSizeLT0
    }

    public enum RuntimeError
    {
        UnknownSyscallHash,
        InvalidAddress,
        OutOfBounds,
        NotOwnerOfBuffer,
        InvalidJumpHash,
        EmptyCallStack,
        EmptyStack,
        CallStackOverflow,
        StackOverflow
    }

    public class BuildErrorException : Exception
    {
        public BuildErrorKind Kind { get; }
        public int? LineNumber { get; }

        public BuildErrorException(BuildErrorKind kind, int? lineNumber = null)
            : base(
                $"Build error: {GetErrorMessage(kind)}"
                    + (lineNumber.HasValue ? $" at line {lineNumber}" : "")
            )
        {
            Kind = kind;
            LineNumber = lineNumber;
        }

        private static string GetErrorMessage(BuildErrorKind kind) =>
            kind switch
            {
                BuildErrorKind.UnknownInstruction => "unknown instruction",
                BuildErrorKind.InvalidParameter => "invalid parameter",
                BuildErrorKind.InvalidOrMissingParameter => "invalid or missing parameter",
                BuildErrorKind.FileSizeLT0 => "invalid file size",
                _ => "unknown error"
            };
    }

    public class RuntimeErrorException : Exception
    {
        public RuntimeError Error { get; }

        public RuntimeErrorException(RuntimeError error)
            : base($"Runtime error: {GetErrorMessage(error)}")
        {
            Error = error;
        }

        private static string GetErrorMessage(RuntimeError error) =>
            error switch
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
                _ => "unknown error"
            };
    }
}

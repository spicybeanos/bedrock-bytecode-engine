

namespace BedrockBytecodeEngine
{
    public interface IVMState;
    public class Running: IVMState;
    public class Halted: IVMState;
    public class Panicked: IVMState
    {
        public ulong ErrorCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public Panicked(ulong errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
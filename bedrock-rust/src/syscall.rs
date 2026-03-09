use crate::error::RuntimeError;
use crate::utils::read_string;
use crate::utils::read_u64;
use crate::vm::VM;

/*
    SyscallH(u64, InstrParam, u8),
    Syscall1(u64, InstrParam, u8),
*/


impl VM {
    pub fn syscallh() -> Result<u64,RuntimeError> {
        Ok(0)
    }
}
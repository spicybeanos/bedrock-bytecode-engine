use crate::{error::RuntimeError, vm::{InstrParam, VM}};
/*
    *Jmp(u64),
    *Jif(u8, u64),
    *Jnif(u8, u64),
    *Call(u64),
    *Ret,
    Ecall(u64, u64),
    *Push(InstrParam),
    *Pop(u8),
*/
impl VM {
    pub fn jmp(&mut self, jmp_hash: u64) -> Result<u64, RuntimeError> {
        let pc : usize = *self
            .label_indexs
            .get(&jmp_hash)
            .ok_or(RuntimeError::InvalidJumpHash)?;
        self.pc = pc;
        Ok(pc as u64)
    }
    pub fn jif(&mut self, r: u8, jmp_hash: u64) -> Result<u64, RuntimeError> {
        let pc : usize = *self
            .label_indexs
            .get(&jmp_hash)
            .ok_or(RuntimeError::InvalidJumpHash)?;
        let cond = self.reg[r as usize];
        if cond == 1 {
            self.pc = pc;
            return Ok(1);
        }
        Ok(0)
    }
    pub fn jnif(&mut self, r: u8, jmp_hash: u64) -> Result<u64, RuntimeError> {
        let pc : usize = *self
            .label_indexs
            .get(&jmp_hash)
            .ok_or(RuntimeError::InvalidJumpHash)?;
        let cond = self.reg[r as usize];
        if cond != 1 {
            self.pc = pc;
            return Ok(1);
        }
        Ok(0)
    }
    pub fn call(&mut self, jmp_hash: u64) -> Result<u64, RuntimeError> {
        let pc: usize = *self
            .label_indexs
            .get(&jmp_hash)
            .ok_or(RuntimeError::InvalidJumpHash)?;
        let old_pc = self.pc;
        self.call_stack.push(old_pc);
        self.pc = pc;
        Ok(old_pc as u64)
    }
    pub fn ret(&mut self) -> Result<u64, RuntimeError> {
        let old_pc = self.call_stack.pop().ok_or(RuntimeError::EmptyCallStack)?;
        self.pc = old_pc;
        Ok(old_pc as u64)
    }
    pub fn push(&mut self, value_p: InstrParam) -> Result<u64, RuntimeError> {
        let value = self.resolve_param(&value_p);
        self.stack.push(value);
        Ok(value)
    }
    pub fn pop(&mut self, r: u8) -> Result<u64, RuntimeError> {
        let value :u64= self.stack.pop().ok_or(RuntimeError::EmptyStack)?;
        self.reg[r as usize] = value;
        Ok(value)
    }
}

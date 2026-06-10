
use std::collections::HashMap;
use crate::vm::Instruction;

/*
Structure of the bytecode:
- 8 bytes: length of the bytecode (excluding these 8 bytes)

*/

pub fn assemble(instrs: Vec<Instruction>,label_indexs: HashMap<u64, usize>) -> Vec<u8> {
    let mut bytecode = vec![0,0,0,0,0,0,0,0];
    return bytecode;
}
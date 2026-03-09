use crate::error::RuntimeError;
use std::cell::RefCell;
use std::collections::HashMap;
use std::rc::Rc;
use std::sync::atomic::AtomicU64;

#[derive(Clone, Copy)]
pub enum InstrParam {
    Register(u8),
    Value(u64),
}

pub enum Instruction {
    Pass,
    // Memory (Local)
    Read(u8, InstrParam, InstrParam),
    Write(u8, InstrParam, InstrParam),
    Check(u8, InstrParam, InstrParam),
    Alloc(u8, InstrParam),
    Realloc(u8, InstrParam),
    Free(InstrParam),
    Vlen(u8, InstrParam),
    ReadW(u8, InstrParam, InstrParam),
    WriteW(u8, InstrParam, InstrParam),

    // Memory (Global Persistent)
    Palloc(u8, InstrParam),
    Prealloc(u8, InstrParam),
    Pfree(InstrParam),
    Pcheck(u8, InstrParam, InstrParam),
    Pread(u8, InstrParam, InstrParam),
    Pwrite(u8, InstrParam, InstrParam),
    PreadW(u8, InstrParam, InstrParam),
    PwriteW(u8, InstrParam, InstrParam),
    Pvlen(u8, InstrParam),
    Powner(u8, InstrParam),

    // Integer Math
    Add(u8, InstrParam),
    Sub(u8, InstrParam),
    Mult(u8, InstrParam),
    Div(u8, InstrParam),
    Mod(u8, InstrParam),
    Lshift(u8, InstrParam),
    Rshift(u8, InstrParam),
    Sqrt(u8, InstrParam),

    // Float Math
    Fadd(u8, InstrParam),
    Fsub(u8, InstrParam),
    Fmult(u8, InstrParam),
    Fdiv(u8, InstrParam),
    Fmod(u8, InstrParam),
    Fsqrt(u8, InstrParam),
    Finv(u8, InstrParam),

    // Logic & Comparison (Int)
    Eq(u8, InstrParam, InstrParam),
    Neq(u8, InstrParam, InstrParam),
    Lt(u8, InstrParam, InstrParam),
    Gt(u8, InstrParam, InstrParam),
    Lte(u8, InstrParam, InstrParam),
    Gte(u8, InstrParam, InstrParam),
    And(u8, InstrParam, InstrParam),
    Or(u8, InstrParam, InstrParam),
    Xor(u8, InstrParam, InstrParam),
    Not(u8, InstrParam),

    // Logic & Comparison (Float)
    Feq(u8, InstrParam, InstrParam),
    Fneq(u8, InstrParam, InstrParam),
    Flt(u8, InstrParam, InstrParam),
    Fgt(u8, InstrParam, InstrParam),
    Flte(u8, InstrParam, InstrParam),
    Fgte(u8, InstrParam, InstrParam),

    // Control Flow
    Jmp(u64),
    Jif(u8, u64),
    Jnif(u8, u64),
    Call(u64),
    Ret,
    Ecall(u64, u64),
    Push(InstrParam),
    Pop(u8),

    // Data & System
    Mov(u8, InstrParam),
    Itof(u8, u8),
    Ftoi(u8, u8),
    SyscallH(u64, InstrParam, u8),
    Syscall1(u64, InstrParam, u8),
}

pub struct PVramEntry {
    pub owner_id: u64,
    pub data: Vec<u8>,
    pub sequence: AtomicU64, // Used for Seqlock synchronization
}

pub struct VM {
    pub id: u64,                       // Unique Mod ID
    pub reg: [u64; 256],               // 64-bit registers
    pub pc: usize,                     // Program Counter
    pub vi_ram: HashMap<u64, Vec<u8>>, // Private memory
    pub label_indexs: HashMap<u64, usize>, // label hash to pc
    pub stack: Vec<u64>,      // Data stack
    pub call_stack: Vec<usize>, // Return addresses
    pub global: Rc<RefCell<GlobalState>>,
}

pub struct GlobalState {
    pub p_vram: HashMap<u64, PVramEntry>,            // Shared memory
    pub attendance: HashMap<u64, HashMap<u64, u64>>, // [ModHash][FuncHash] -> PC
}

impl GlobalState {
    pub fn new_shared() -> Rc<RefCell<GlobalState>> {
        Rc::new(RefCell::new(GlobalState {
            p_vram: HashMap::new(),
            attendance: HashMap::new(),
        }))
    }
}

impl VM {
    pub fn new(id: u64, global: Rc<RefCell<GlobalState>>) -> VM {
        return VM {
            reg: std::array::from_fn(|_| 0),
            pc: 0,
            id,
            vi_ram: HashMap::new(),
            label_indexs: HashMap::new(),
            stack: Vec::new(),
            call_stack: Vec::new(),
            global,
        };
    }
    pub fn run(&mut self, instr: &Vec<Instruction>, start: usize) -> Result<u64, RuntimeError> {
        self.pc = start;
        let len = instr.len();

        while self.pc < len {
            self.execute(instr)?;
        }

        return Ok(0);
    }
    pub fn execute(&mut self, instr: &Vec<Instruction>) -> Result<u64, RuntimeError> {
        let work = &instr[self.pc as usize];
        self.pc += 1;
        match work {
            //maths
            Instruction::Add(r, v2) => Ok(self.add(*r, v2)),
            Instruction::Fadd(r, v2) => Ok(self.fadd(*r, v2)),
            Instruction::Finv(r, v2) => Ok(self.finv(*r, v2)),
            Instruction::Sub(r, v2) => Ok(self.sub(*r, v2)),
            Instruction::Fsub(r, v2) => Ok(self.fsub(*r, v2)),
            Instruction::Mult(r, v2) => Ok(self.mult(*r, v2)),
            Instruction::Fmult(r, v2) => Ok(self.fmult(*r, v2)),
            Instruction::Div(r, v2) => Ok(self.div(*r, v2)),
            Instruction::Fdiv(r, v2) => Ok(self.fdiv(*r, v2)),
            Instruction::Mod(r, v2) => Ok(self.modulous(*r, v2)),
            Instruction::Fmod(r, v2) => Ok(self.fmodulous(*r, v2)),
            Instruction::Lshift(r, v2) => Ok(self.lshift(*r, v2)),
            Instruction::Rshift(r, v2) => Ok(self.rshift(*r, v2)),
            Instruction::Mov(r, v2) => Ok(self.mov(*r, v2)),
            Instruction::Sqrt(r, v2) => Ok(self.sqrt(*r, v2)),
            Instruction::Fsqrt(r, v2) => Ok(self.fsqrt(*r, v2)),

            Instruction::Itof(r, v2) => Ok(self.itof(*r, *v2)),
            Instruction::Ftoi(r, v2) => Ok(self.ftoi(*r, *v2)),

            Instruction::Eq(r, v1, v2) => Ok(self.eq(*r, v1, v2)),
            Instruction::Neq(r, v1, v2) => Ok(self.neq(*r, v1, v2)),
            Instruction::Gt(r, v1, v2) => Ok(self.gt(*r, v1, v2)),
            Instruction::Lt(r, v1, v2) => Ok(self.lt(*r, v1, v2)),
            Instruction::Gte(r, v1, v2) => Ok(self.gte(*r, v1, v2)),
            Instruction::Lte(r, v1, v2) => Ok(self.lte(*r, v1, v2)),

            Instruction::And(r, v1, v2) => Ok(self.and(*r, v1, v2)),
            Instruction::Or(r, v1, v2) => Ok(self.or(*r, v1, v2)),
            Instruction::Xor(r, v1, v2) => Ok(self.xor(*r, v1, v2)),
            Instruction::Not(r, v1) => Ok(self.not(*r, v1)),

            Instruction::Feq(r, v1, v2) => Ok(self.feq(*r, v1, v2)),
            Instruction::Fneq(r, v1, v2) => Ok(self.fneq(*r, v1, v2)),
            Instruction::Fgt(r, v1, v2) => Ok(self.fgt(*r, v1, v2)),
            Instruction::Flt(r, v1, v2) => Ok(self.flt(*r, v1, v2)),
            Instruction::Fgte(r, v1, v2) => Ok(self.fgte(*r, v1, v2)),
            Instruction::Flte(r, v1, v2) => Ok(self.flte(*r, v1, v2)),

            //memory
            Instruction::Read(r, addr, k) => self.read(*r, addr, k),
            Instruction::Write(r, addr, k) => self.write(*r, addr, k),
            Instruction::Check(r, addr, k) => self.check(*r, addr, k),
            Instruction::Alloc(r, size) => Ok(self.alloc(*r, size)),
            Instruction::Realloc(r, size) => self.realloc(*r, size),
            Instruction::Free(addr) => self.free(addr),
            Instruction::Vlen(r, loc) => Ok(self.vlen(*r, loc)),
            Instruction::ReadW(r, addr, k) => self.readw(*r, addr, k),
            Instruction::WriteW(r, addr, k) => self.writew(*r, addr, k),

            Instruction::Palloc(r, size) => Ok(self.palloc(*r, size)),
            Instruction::Prealloc(r, size) => self.prealloc(*r, size),
            Instruction::Pfree(r) => self.pfree(r),
            Instruction::Pcheck(r, addr, k) => self.pcheck(*r, addr, k),
            Instruction::Pread(r, addr, k) => self.pread(*r, addr, k),
            Instruction::Pwrite(r, addr, k) => self.pwrite(*r, addr, k),
            Instruction::PreadW(r, addr, k) => self.preadw(*r, addr, k),
            Instruction::PwriteW(r, addr, k) => self.pwritew(*r, addr, k),
            Instruction::Pvlen(r, loc) => Ok(self.pvlen(*r, loc)),
            Instruction::Powner(r, loc) => self.powner(*r, loc),

            //control flow
            Instruction::Jmp(jmp_hash) => self.jmp(*jmp_hash),
            Instruction::Jif(r, jmp_hash) => self.jif(*r, *jmp_hash),
            Instruction::Jnif(r, jmp_hash) => self.jnif(*r, *jmp_hash),
            Instruction::Call(jmp_hash) => self.call(*jmp_hash),
            Instruction::Ret => self.ret(),
            Instruction::Push(value_p) => self.push(*value_p),
            Instruction::Pop(r) => self.pop(*r),

            Instruction::Pass => Ok(0),

            _ => Ok(0),
        }
    }
}

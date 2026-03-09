use crate::vm::{GlobalState, InstrParam, Instruction, VM};

pub fn run_math_test() {
    let instr = vec![
        Instruction::Mov(1, InstrParam::Value(20)),
        Instruction::Mov(2, InstrParam::Value(20)),
        Instruction::Mult(1, InstrParam::Register(2)),
    ];

    let global = GlobalState::new_shared();
    let mut vm = VM::new(1, global);
    let _ = vm.run(&instr, 0);

    println!("value of r1 = {}", vm.reg[1]);
}

pub fn run_memory_test() {
    let instr = vec![
        Instruction::Mov(1, InstrParam::Value(400)),
        Instruction::Alloc(3, InstrParam::Value(8)),
        Instruction::WriteW(1, InstrParam::Register(3), InstrParam::Value(0)),
        Instruction::ReadW(4, InstrParam::Register(3), InstrParam::Value(0)),
    ];

    let global = GlobalState::new_shared();
    let mut vm = VM::new(1, global);
    let _ = vm.run(&instr, 0);

    println!("value of r4 = {}", vm.reg[4]);
    println!("location of alloc = {:x}", vm.reg[3]);

    for (addr, bytes) in &vm.vi_ram {
        println!("addr {}:", addr);
        for b in bytes {
            print!("{}-", b);
        }
        println!();
    }
}

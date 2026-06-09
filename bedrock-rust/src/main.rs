mod utils;
mod vm;
use crate::scanner::construct_from_text;
use std::fs;
use std::path::Path;
mod control;
mod error;
mod scanner;
mod math;
mod memory;
mod engine;
mod syscall;
mod test;
mod source;
mod vm_state;

fn dump_first_10_registers(vm: &vm::VM) {
    println!("register dump (r0-r9):");
    for i in 0..10 {
        let value = vm.reg[i];
        println!("r{} = {} (0x{:X})", i, value, value);
    }
}

fn dump_vi_ram(vm: &vm::VM) {
    println!("vi_ram memdump:");
    if vm.vi_ram.is_empty() {
        println!("<empty>");
        return;
    }

    let mut addrs = vm.vi_ram.keys().copied().collect::<Vec<u64>>();
    addrs.sort_unstable();

    for addr in addrs {
        if let Some(bytes) = vm.vi_ram.get(&addr) {
            print!("addr {} (0x{:X}) len {}: ", addr, addr, bytes.len());
            for (idx, byte) in bytes.iter().enumerate() {
                if idx > 0 {
                    print!(" ");
                }
                print!("{:02X}", byte);
            }
            println!();
        }
    }
}

fn dump_p_vram(vm: &vm::VM) {
    println!("p_vram memdump:");
    let global = vm.global.borrow();
    if global.p_vram.is_empty() {
        println!("<empty>");
        return;
    }

    let mut addrs = global.p_vram.keys().copied().collect::<Vec<u64>>();
    addrs.sort_unstable();

    for addr in addrs {
        if let Some(entry) = global.p_vram.get(&addr) {
            print!(
                "addr {} (0x{:X}) owner {} len {}: ",
                addr,
                addr,
                entry.owner_id,
                entry.data.len()
            );
            for (idx, byte) in entry.data.iter().enumerate() {
                if idx > 0 {
                    print!(" ");
                }
                print!("{:02X}", byte);
            }
            println!();
        }
    }
}

fn build_error_name(kind: error::BuildErrorKind) -> &'static str {
    match kind {
        error::BuildErrorKind::UnkownInstruction => "unknown instruction",
        error::BuildErrorKind::InvalidParameter => "invalid parameter",
        error::BuildErrorKind::InvalidOrMissingParameter => "invalid or missing parameter",
        error::BuildErrorKind::FileSizeLT0 => "invalid file size",
        error::BuildErrorKind::DuplicateVMId => "duplicate VM ID",
    }
}

fn runtime_error_name(err: &error::RuntimeError) -> String {
    match err {
        error::RuntimeError::UnkownSyscallHash => "unknown syscall hash".to_owned(),
        error::RuntimeError::InvalidAddress => "invalid address".to_owned(),
        error::RuntimeError::OutOfBounds => "out of bounds".to_owned(),
        error::RuntimeError::NotOwnerOfBuffer => "not owner of buffer".to_owned(),
        error::RuntimeError::InvalidJumpHash => "invalid jump label hash".to_owned(),
        error::RuntimeError::EmptyCallStack => "Empty call stack".to_owned(),
        error::RuntimeError::EmptyStack => "Empty stack".to_owned(),
        error::RuntimeError::CallStackOverflow => "call stack overflow".to_owned(),
        error::RuntimeError::StackOverflow => "stack overflow".to_owned(),
        error::RuntimeError::Panic(code,msg ) => format!("panicked <{}>: {}", code, msg),
    }
}

fn main() {
    let path: String = match std::env::args().nth(1) {
        Some(path) => path,
        _ => {
            test::print_hashes();
            return;
        }
    };

    let is_bdt = Path::new(&path)
        .extension()
        .and_then(|ext| ext.to_str())
        .map(|ext: &str| ext.eq_ignore_ascii_case("bdt"))
        .unwrap_or(false);
    if !is_bdt {
        println!("extension incorrect");
        return;
    }

    let content = match fs::read_to_string(&path) {
        Ok(content) => content,
        Err(_) => {
            println!("failed to read file");
            return;
        }
    };

    let text = content.chars().collect::<Vec<char>>();
    let global = vm::GlobalState::new_shared();

    let (mut vm, instructions, label_indexs) = match construct_from_text(1, global, &text) {
        Ok(data) => data,
        Err(err) => {
            if let Some(line) = err.line() {
                println!("build error on line {}: {}", line, build_error_name(err.kind()));
            } else {
                println!("build error: {}", build_error_name(err.kind()));
            }
            return;
        }
    };

    if let Err(err) = vm.run(&instructions, &label_indexs, 0) {
        println!("runtime error: {}", runtime_error_name(&err));
    }

    dump_first_10_registers(&vm);
    dump_vi_ram(&vm);
    dump_p_vram(&vm);
}

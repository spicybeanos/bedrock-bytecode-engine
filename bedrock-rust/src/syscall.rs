use crate::error::RuntimeError;
use crate::utils::read_string;
use crate::utils::read_u64;
use crate::vm::VM;

pub fn syscallh(vm: &mut VM, func_hash: u64, params: u64, ret: u64) -> Result<u64, RuntimeError> {
    match func_hash {
        //print
        3388837930293092456 => {
            let o_params_buff = &vm.vi_ram.get(&params);
            match o_params_buff {
                Option::Some(params_buff) => {
                    let str_ = read_string(params_buff, 0, params_buff.len());
                    println!("{}", str_);
                    return Ok(0);
                }
                Option::None => return Err(RuntimeError::InvalidAddress),
            }
        }
        _ => {
            return Err(RuntimeError::UnkownSyscallHash);
        }
    }
}

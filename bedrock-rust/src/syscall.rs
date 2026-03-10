use crate::error::RuntimeError;
use crate::utils::read_f64;
use crate::utils::read_i64;
use crate::utils::read_string;
use crate::utils::read_u64;
use crate::vm::InstrParam;
use crate::vm::VM;

/*
    SyscallH(u64, InstrParam, u8),
    Syscall1(u64, InstrParam, u8),
*/

impl VM {
    pub fn syscallh(
        &mut self,
        hash_p: u64,
        handle_p: InstrParam,
        dest_p: InstrParam,
    ) -> Result<u64, RuntimeError> {
        let hash = hash_p;
        let handle_addr = self.resolve_param(&handle_p);
        let dest_addr = self.resolve_param(&dest_p);

        match hash {
            //io.console.print -> 10634550992374585907 (939583C88643DA33)
            0x939583C88643DA33 => {
                let buff = self
                    .vi_ram
                    .get(&handle_addr)
                    .ok_or(RuntimeError::InvalidAddress)?;
                let p = String::from_utf8(buff.to_vec());
                match p {
                    Ok(str) => {
                        print!("{}", str);
                    }
                    Err(e) => {
                        print!("utf8-err-{}", e);
                    }
                }
            }
            //io.console.read_line -> 11174140223897499719 (9B1285549BD07847)
            
            //io.file.read -> 4100346326675936995 (38E75B427198D6E3)
            //io.file.write -> 5796946121189188610 (5072E3C89F8F6402)
            //io.file.append -> 6067790551616150519 (54351F5BB08D5BF7)
            //io.file.exists -> 3578709093477348543 (31AA20F5AF2D74BF)
            //io.file.length -> 13002782639914582063 (B47329EB57B9802F)

            //u64.from_string -> 6853399315506211892 (5F1C2A5DD8A72C34)
            //i64.from_string -> 16076306678625843816 (DF1A83BF8FBBBA68)
            //f64.from_string -> 6978109133763550277 (60D7394919E29C45)

            //u64.to_string -> 8149430780573344407 (7118981E0DB56297)
            8149430780573344407 => {
                let buff = self
                    .vi_ram
                    .get(&handle_addr)
                    .ok_or(RuntimeError::InvalidAddress)?;
                let str = read_u64(buff, 0).to_string();
                let bytes = str.as_bytes();
                let ret_b = self.vi_ram.get_mut(&dest_addr);
                match ret_b {
                    Some(ret_buff) => {
                        if ret_buff.len() > bytes.len() {
                            ret_buff.copy_from_slice(&bytes);
                        } else {
                            self.vi_ram.remove(&dest_addr);
                            self.vi_ram.insert(dest_addr, bytes.to_vec());
                        }
                    }
                    _ => {
                        return Err(RuntimeError::InvalidAddress);
                    }
                }
            }
            //i64.to_string -> 15461467554190651459 (D6922AD279C2F043)
            15461467554190651459 => {
                let buff = self
                    .vi_ram
                    .get(&handle_addr)
                    .ok_or(RuntimeError::InvalidAddress)?;
                let str = read_i64(buff, 0).to_string();
                let bytes = str.as_bytes();
                let ret_b = self.vi_ram.get_mut(&dest_addr);
                match ret_b {
                    Some(ret_buff) => {
                        if ret_buff.len() > bytes.len() {
                            ret_buff.copy_from_slice(&bytes);
                        } else {
                            self.vi_ram.remove(&dest_addr);
                            self.vi_ram.insert(dest_addr, bytes.to_vec());
                        }
                    }
                    _ => {
                        return Err(RuntimeError::InvalidAddress);
                    }
                }
            }
            //f64.to_string -> 417476877239542410 (5CB2D0209CD628A)
            417476877239542410 => {
                let buff = self
                    .vi_ram
                    .get(&handle_addr)
                    .ok_or(RuntimeError::InvalidAddress)?;
                let str = read_f64(buff, 0).to_string();
                let bytes = str.as_bytes();
                let ret_b = self.vi_ram.get_mut(&dest_addr);
                match ret_b {
                    Some(ret_buff) => {
                        if ret_buff.len() > bytes.len() {
                            ret_buff.copy_from_slice(&bytes);
                        } else {
                            self.vi_ram.remove(&dest_addr);
                            self.vi_ram.insert(dest_addr, bytes.to_vec());
                        }
                    }
                    _ => {
                        return Err(RuntimeError::InvalidAddress);
                    }
                }
            }
            _ => {
                return Err(RuntimeError::UnkownSyscallHash);
            }
        }

        Ok(0)
    }
}

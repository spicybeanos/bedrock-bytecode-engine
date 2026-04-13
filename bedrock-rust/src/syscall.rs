use crate::error::RuntimeError;
use crate::utils::read_f64;
use crate::utils::read_i64;
use crate::utils::read_u64;
use crate::vm::InstrParam;
use crate::vm::VM;
use std::fs::{self, OpenOptions};
use std::io::{self, Write};
use std::path::Path;

/*
    SyscallH(u64, InstrParam, u8),
    Syscall1(u64, InstrParam, u8),
*/

impl VM {
    fn read_buffer_string(&self, addr: u64) -> Result<String, RuntimeError> {
        let buff = self
            .vi_ram
            .get(&addr)
            .ok_or(RuntimeError::InvalidAddress)?;
        Ok(String::from_utf8_lossy(buff)
            .trim_end_matches('\0')
            .to_string())
    }

    fn write_buffer_bytes(&mut self, addr: u64, bytes: &[u8]) -> Result<u64, RuntimeError> {
        let buff = self
            .vi_ram
            .get_mut(&addr)
            .ok_or(RuntimeError::InvalidAddress)?;
        buff.clear();
        buff.extend_from_slice(bytes);
        Ok(bytes.len() as u64)
    }

    fn write_buffer_u64(&mut self, addr: u64, value: u64) -> Result<u64, RuntimeError> {
        self.write_buffer_bytes(addr, &value.to_le_bytes())
    }

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
                let text = self.read_buffer_string(handle_addr)?;
                print!("{}", text);
                let _ = io::stdout().flush();
            }
            //io.console.read_line -> 11174140223897499719 (9B1285549BD07847)
            0x9B1285549BD07847 => {
                let prompt = self.read_buffer_string(handle_addr)?;
                if !prompt.is_empty() {
                    print!("{}", prompt);
                    let _ = io::stdout().flush();
                }

                let mut input = String::new();
                io::stdin()
                    .read_line(&mut input)
                    .map_err(|_| RuntimeError::InvalidAddress)?;
                while input.ends_with('\n') || input.ends_with('\r') {
                    input.pop();
                }
                self.write_buffer_bytes(dest_addr, input.as_bytes())?;
            }

            //io.file.read -> 4100346326675936995 (38E75B427198D6E3)
            0x38E75B427198D6E3 => {
                let path = self.read_buffer_string(handle_addr)?;
                let bytes = fs::read(Path::new(&path)).map_err(|_| RuntimeError::InvalidAddress)?;
                self.write_buffer_bytes(dest_addr, &bytes)?;
            }

            //io.file.write -> 5796946121189188610 (5072E3C89F8F6402)
            0x5072E3C89F8F6402 => {
                let path = self.read_buffer_string(handle_addr)?;
                let data = self
                    .vi_ram
                    .get(&dest_addr)
                    .ok_or(RuntimeError::InvalidAddress)?
                    .clone();
                fs::write(Path::new(&path), data).map_err(|_| RuntimeError::InvalidAddress)?;
            }

            //io.file.append -> 6067790551616150519 (54351F5BB08D5BF7)
            0x54351F5BB08D5BF7 => {
                let path = self.read_buffer_string(handle_addr)?;
                let data = self
                    .vi_ram
                    .get(&dest_addr)
                    .ok_or(RuntimeError::InvalidAddress)?
                    .clone();
                let mut file = OpenOptions::new()
                    .create(true)
                    .append(true)
                    .open(Path::new(&path))
                    .map_err(|_| RuntimeError::InvalidAddress)?;
                file.write_all(&data)
                    .map_err(|_| RuntimeError::InvalidAddress)?;
            }

            //io.file.exists -> 3578709093477348543 (31AA20F5AF2D74BF)
            0x31AA20F5AF2D74BF => {
                let path = self.read_buffer_string(handle_addr)?;
                let exists = Path::new(&path).exists() as u64;
                self.write_buffer_u64(dest_addr, exists)?;
            }

            //io.file.length -> 13002782639914582063 (B47329EB57B9802F)
            0xB47329EB57B9802F => {
                let path = self.read_buffer_string(handle_addr)?;
                let len = fs::metadata(Path::new(&path))
                    .map_err(|_| RuntimeError::InvalidAddress)?
                    .len();
                self.write_buffer_u64(dest_addr, len)?;
            }

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

    pub fn syscall1(
        &mut self,
        hash_p: u64,
        arg_p: InstrParam,
        dest_p: InstrParam,
    ) -> Result<u64, RuntimeError> {
        let hash = hash_p;
        let arg_addr = self.resolve_param(&arg_p);
        let dest_addr = self.resolve_param(&dest_p);

        match hash {
            //io.console.read_line -> 11174140223897499719 (9B1285549BD07847)
            0x9B1285549BD07847 => {
                let prompt = self.read_buffer_string(arg_addr)?;
                if !prompt.is_empty() {
                    print!("{}", prompt);
                    let _ = io::stdout().flush();
                }

                let mut input = String::new();
                io::stdin()
                    .read_line(&mut input)
                    .map_err(|_| RuntimeError::InvalidAddress)?;
                while input.ends_with('\n') || input.ends_with('\r') {
                    input.pop();
                }
                self.write_buffer_bytes(dest_addr, input.as_bytes())?;
            }

            //u64.from_string -> 6853399315506211892 (5F1C2A5DD8A72C34)
            0x5F1C2A5DD8A72C34 => {
                let text = self.read_buffer_string(arg_addr)?;
                let value = text.trim().parse::<u64>().unwrap_or(0);
                self.write_buffer_u64(dest_addr, value)?;
            }

            //i64.from_string -> 16076306678625843816 (DF1A83BF8FBBBA68)
            0xDF1A83BF8FBBBA68 => {
                let text = self.read_buffer_string(arg_addr)?;
                let value = text.trim().parse::<i64>().unwrap_or(0);
                self.write_buffer_bytes(dest_addr, &value.to_le_bytes())?;
            }

            //f64.from_string -> 6978109133763550277 (60D7394919E29C45)
            0x60D7394919E29C45 => {
                let text = self.read_buffer_string(arg_addr)?;
                let value = text.trim().parse::<f64>().unwrap_or(0.0);
                self.write_buffer_bytes(dest_addr, &value.to_le_bytes())?;
            }

            _ => {
                return Err(RuntimeError::UnkownSyscallHash);
            }
        }

        Ok(0)
    }
}

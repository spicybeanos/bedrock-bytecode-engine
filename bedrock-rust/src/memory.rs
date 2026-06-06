use crate::{ error::RuntimeError, vm::{ InstrParam, PVramEntry, VM } };
use std::cmp;
use crate::utils::hash_u64_vm;
/*
    *Read(u8, InstrParam, InstrParam),
    *Write(u8, InstrParam, InstrParam),
    *Check(u8, InstrParam, InstrParam),
    *Alloc(u8, InstrParam),
    *Realloc(u8, InstrParam),
    *Free(InstrParam),
    *Vlen(u8,InstrParam),

    // Memory (Global Persistent)
    *Palloc(u8, InstrParam),
    *Prealloc(u8, InstrParam),
    *Pfree(u8),
    *Pread(u8, InstrParam, InstrParam),
    *Pcheck(u8, InstrParam, InstrParam),
    *Pwrite(u8, InstrParam, InstrParam),
    *Pvlen(u8,InstrParam),
    *Powner(u8,InstrParam)
*/

impl VM {
    pub fn resolve_param(&self, param: &InstrParam) -> u64 {
        match param {
            InstrParam::Register(r) => self.reg[*r as usize],
            InstrParam::Value(v) => *v,
        }
    }
    pub fn mov(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] = val;
        return self.reg[r as usize];
    }

    //local memory
    pub fn read(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let buff = self.vi_ram.get(&addr).ok_or(RuntimeError::InvalidAddress)?;
        let val = *buff.get(k as usize).ok_or(RuntimeError::OutOfBounds)? as u64;
        self.reg[r as usize] = val;
        Ok(val)
    }
    pub fn write(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let buff = self.vi_ram.get_mut(&addr).ok_or(RuntimeError::InvalidAddress)?;
        if k + 8 > buff.len() {
            return Err(RuntimeError::OutOfBounds);
        }
        buff[k as usize] = self.reg[r as usize] as u8;
        Ok(self.reg[r as usize])
    }
    pub fn readw(
        &mut self,
        r: u8, // Destination Register
        addr_p: &InstrParam, // ViRam Handle
        k_p: &InstrParam // Byte Offset
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let buff = self.vi_ram.get(&addr).ok_or(RuntimeError::InvalidAddress)?;

        // Safety: Check if there are at least 8 bytes available from offset k
        if k + 8 > buff.len() {
            return Err(RuntimeError::OutOfBounds);
        }

        // Extract 8 bytes and convert to u64 (Little Endian)
        let mut bytes = [0u8; 8];
        bytes.copy_from_slice(&buff[k..k + 8]);
        let val = u64::from_le_bytes(bytes);

        self.reg[r as usize] = val;
        Ok(val)
    }
    pub fn writew(
        &mut self,
        r: u8, // Source Register
        addr_p: &InstrParam, // ViRam Handle
        k_p: &InstrParam // Byte Offset
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let buff = self.vi_ram.get_mut(&addr).ok_or(RuntimeError::InvalidAddress)?;

        // Safety: Check if the buffer is large enough to receive 8 bytes
        if k + 8 > buff.len() {
            return Err(RuntimeError::OutOfBounds);
        }

        // Convert register u64 to bytes and copy into ViRam
        let val = self.reg[r as usize];
        let bytes = val.to_le_bytes();
        buff[k..k + 8].copy_from_slice(&bytes);

        Ok(val)
    }
    pub fn check(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = match addr_p {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(a) => *a,
        };
        let k = match k_p {
            InstrParam::Register(rk) => self.reg[*rk as usize],
            InstrParam::Value(v) => *v,
        };

        match self.vi_ram.get(&addr) {
            Option::Some(buff) => {
                if (buff.len() as u64) > k {
                    self.reg[r as usize] = 1;
                    Ok(1)
                } else {
                    self.reg[r as usize] = 0;
                    Ok(0)
                }
            }
            Option::None => Ok(0),
        }
    }
    pub fn vlen(&mut self, r: u8, loc_p: &InstrParam) -> u64 {
        let loc = self.resolve_param(loc_p);

        match self.vi_ram.get(&loc) {
            Option::Some(buff) => {
                self.reg[r as usize] = buff.len() as u64;
                return self.reg[r as usize];
            }
            Option::None => {
                self.reg[r as usize] = 0;
                return 0;
            }
        };
    }
    pub fn alloc(&mut self, r: u8, size_p: &InstrParam) -> u64 {
        let size = self.resolve_param(size_p);
        let addr = hash_u64_vm(self.next_handle);
        
        self.next_handle += 1;
        self.vi_ram.insert(addr, vec![0; size as usize]);
        self.reg[r as usize] = addr;

        return 0;
    }
    pub fn realloc(&mut self, r: u8, size_p: &InstrParam) -> Result<u64, RuntimeError> {
        let size = self.resolve_param(size_p);
        let mut new_vec = vec![0; size as usize];
        let old = self.vi_ram
            .get(&self.reg[r as usize])
            .ok_or(RuntimeError::InvalidAddress)?
            .to_owned();
        let len = cmp::min(size as usize, old.len());
        let mut i = 0;
        while i < len {
            new_vec[i] = old[i];
            i += 1;
        }
        self.vi_ram.remove(&self.reg[r as usize]);
        self.vi_ram.insert(self.reg[r as usize], new_vec);
        return Ok(0);
    }
    pub fn free(&mut self, addr_p: &InstrParam) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        self.vi_ram.remove(&addr);
        return Ok(0);
    }
    //public memory
    pub fn pread(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let global = self.global.borrow();
        let entry = global.p_vram.get(&addr).ok_or(RuntimeError::InvalidAddress)?;
        let val = *entry.data.get(k as usize).ok_or(RuntimeError::OutOfBounds)? as u64;
        self.reg[r as usize] = val;
        Ok(val)
    }
    pub fn pwrite(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let mut global = self.global.borrow_mut();
        let buff = global.p_vram.get_mut(&addr).ok_or(RuntimeError::InvalidAddress)?;
        if buff.owner_id != self.id {
            return Err(RuntimeError::NotOwnerOfBuffer);
        }
        buff.data[k as usize] = self.reg[r as usize] as u8;
        Ok(self.reg[r as usize])
    }
    pub fn preadw(
        &mut self,
        r: u8, // Destination Register
        addr_p: &InstrParam, // ViRam Handle
        k_p: &InstrParam // Byte Offset
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let global = self.global.borrow();
        let buff = global.p_vram.get(&addr).ok_or(RuntimeError::InvalidAddress)?;

        // Safety: Check if there are at least 8 bytes available from offset k
        if k + 8 > buff.data.len() {
            return Err(RuntimeError::OutOfBounds);
        }

        // Extract 8 bytes and convert to u64 (Little Endian)
        let mut bytes = [0u8; 8];
        bytes.copy_from_slice(&buff.data[k..k + 8]);
        let val = u64::from_le_bytes(bytes);

        self.reg[r as usize] = val;
        Ok(val)
    }
    pub fn pwritew(
        &mut self,
        r: u8, // Source Register
        addr_p: &InstrParam, // ViRam Handle
        k_p: &InstrParam // Byte Offset
    ) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let k = self.resolve_param(k_p) as usize;

        let mut global = self.global.borrow_mut();
        let buff = global.p_vram.get_mut(&addr).ok_or(RuntimeError::InvalidAddress)?;
        if buff.owner_id != self.id {
            return Err(RuntimeError::NotOwnerOfBuffer);
        }

        // Safety: Check if the buffer is large enough to receive 8 bytes
        if k + 8 > buff.data.len() {
            return Err(RuntimeError::OutOfBounds);
        }

        // Convert register u64 to bytes and copy into ViRam
        let val = self.reg[r as usize];
        let bytes = val.to_le_bytes();
        buff.data[k..k + 8].copy_from_slice(&bytes);

        Ok(val)
    }
    pub fn pcheck(
        &mut self,
        r: u8,
        addr_p: &InstrParam,
        k_p: &InstrParam
    ) -> Result<u64, RuntimeError> {
        let addr = match addr_p {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(a) => *a,
        };
        let k = match k_p {
            InstrParam::Register(rk) => self.reg[*rk as usize],
            InstrParam::Value(v) => *v,
        };

        let global = self.global.borrow();

        match global.p_vram.get(&addr) {
            Option::Some(buff) => {
                if (buff.data.len() as u64) > k {
                    self.reg[r as usize] = 1;
                    Ok(1)
                } else {
                    self.reg[r as usize] = 0;
                    Ok(0)
                }
            }
            Option::None => Ok(0),
        }
    }
    pub fn pvlen(&mut self, r: u8, loc_p: &InstrParam) -> u64 {
        let loc = self.resolve_param(loc_p);
        let global = self.global.borrow();
        match global.p_vram.get(&loc) {
            Option::Some(buff) => {
                self.reg[r as usize] = buff.data.len() as u64;
                return self.reg[r as usize];
            }
            Option::None => {
                self.reg[r as usize] = 0;
                return 0;
            }
        };
    }
    pub fn powner(&mut self, r: u8, loc_p: &InstrParam) -> Result<u64, RuntimeError> {
        let loc = self.resolve_param(loc_p);
        let global = self.global.borrow();
        match global.p_vram.get(&loc) {
            Option::Some(buff) => {
                self.reg[r as usize] = buff.owner_id;
                return Ok(self.reg[r as usize]);
            }
            Option::None => {
                return Err(RuntimeError::InvalidAddress);
            }
        };
    }
    pub fn palloc(&mut self, r: u8, size_p: &InstrParam) -> u64 {
        let size = self.resolve_param(size_p);
        let mut global = self.global.borrow_mut();
        let addr = hash_u64_vm(global.next_global_handle);
        global.next_global_handle += 1;

        global.p_vram.insert(addr, PVramEntry {
            data: vec![0; size as usize],
            owner_id: self.id,
        });
        self.reg[r as usize] = addr;
        return 0;
    }
    pub fn prealloc(&mut self, r: u8, size_p: &InstrParam) -> Result<u64, RuntimeError> {
        let size = self.resolve_param(size_p);
        let mut global = self.global.borrow_mut();
        let old = global.p_vram
            .get(&self.reg[r as usize])
            .ok_or(RuntimeError::InvalidAddress)?
            .to_owned();
        if old.owner_id != self.id {
            return Err(RuntimeError::NotOwnerOfBuffer);
        }

        let mut new_vec = vec![0; size as usize];
        let len = cmp::min(size as usize, old.data.len());
        let mut i = 0;
        while i < len {
            new_vec[i] = old.data[i];
            i += 1;
        }
        global.p_vram.remove(&self.reg[r as usize]);
        global.p_vram.insert(self.reg[r as usize], PVramEntry {
            data: new_vec,
            owner_id: self.id,
        });
        return Ok(0);
    }
    pub fn pfree(&mut self, addr_p: &InstrParam) -> Result<u64, RuntimeError> {
        let addr = self.resolve_param(addr_p);
        let mut global = self.global.borrow_mut();
        let owner = global.p_vram.get(&addr).ok_or(RuntimeError::InvalidAddress)?;
        if owner.owner_id != self.id {
            return Err(RuntimeError::NotOwnerOfBuffer);
        }
        global.p_vram.remove(&addr);

        return Ok(0);
    }
}

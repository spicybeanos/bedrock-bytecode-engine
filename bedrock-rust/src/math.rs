use crate::vm::{InstrParam, VM};

impl VM {
    pub fn add(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] += val;
        return self.reg[r as usize];
    }
    pub fn fadd(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::from_bits(self.reg[r as usize]) + val;
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn sub(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] -= val;
        return self.reg[r as usize];
    }
    pub fn fsub(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::from_bits(self.reg[r as usize]) - val;
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn mult(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] *= val;
        return self.reg[r as usize];
    }
    pub fn fmult(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::from_bits(self.reg[r as usize]) * val;
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn div(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] /= val;
        return self.reg[r as usize];
    }
    pub fn fdiv(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::from_bits(self.reg[r as usize]) / val;
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn sqrt(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] = f64::sqrt(val as f64) as u64;
        return self.reg[r as usize];
    }
    pub fn fsqrt(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::sqrt(val);
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn modulous(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] %= val;
        return self.reg[r as usize];
    }
    pub fn fmodulous(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => f64::from_bits(self.reg[*r2 as usize]),
            InstrParam::Value(v) => f64::from_bits(*v),
        };
        let res = f64::from_bits(self.reg[r as usize]) % val;
        self.reg[r as usize] = res.to_bits();
        return self.reg[r as usize];
    }

    pub fn lshift(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] <<= val;
        return self.reg[r as usize];
    }
    pub fn rshift(&mut self, r: u8, v2: &InstrParam) -> u64 {
        let val = match v2 {
            InstrParam::Register(r2) => self.reg[*r2 as usize],
            InstrParam::Value(v) => *v,
        };
        self.reg[r as usize] >>= val;
        return self.reg[r as usize];
    }


    pub fn ftoi(&mut self, r: u8, r2: u8) -> u64 {
        let i = f64::from_bits(self.reg[r2 as usize]) as u64;
        self.reg[r as usize] = i;
        return i;
    }
    pub fn itof(&mut self, r: u8, r2: u8) -> u64 {
        self.reg[r as usize] = (self.reg[r2 as usize] as f64).to_bits();
        return self.reg[r as usize];
    }

    pub fn eq(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 == val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn neq(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 != val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn gt(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 > val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn lt(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 < val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn gte(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 >= val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn lte(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        if val1 <= val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }

    pub fn and(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };

        self.reg[r as usize] = val1 & val2;
        return self.reg[r as usize];
    }
    pub fn or(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };

        self.reg[r as usize] = val1 | val2;
        return self.reg[r as usize];
    }
    pub fn xor(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };

        self.reg[r as usize] = val1 ^ val2;
        return self.reg[r as usize];
    }
    pub fn not(&mut self, r: u8, v1: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => self.reg[*r1 as usize],
            InstrParam::Value(p) => *p,
        };

        self.reg[r as usize] = !val1;
        return self.reg[r as usize];
    }

    pub fn feq(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 == val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn fneq(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 != val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn fgt(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 > val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn flt(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 < val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn fgte(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 >= val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
    pub fn flte(&mut self, r: u8, v1: &InstrParam, v2: &InstrParam) -> u64 {
        let val1 = match v1 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        let val2 = match v2 {
            InstrParam::Register(r1) => f64::from_bits(self.reg[*r1 as usize]),
            InstrParam::Value(p) => f64::from_bits(*p),
        };
        if val1 <= val2 {
            self.reg[r as usize] = 1;
            1
        } else {
            self.reg[r as usize] = 0;
            0
        }
    }
}

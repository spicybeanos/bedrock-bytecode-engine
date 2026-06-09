use std::collections::HashMap;

use crate::{utils, vm::Instruction};



pub struct Source {
    pub name: String,
    pub source_file_path: String,
    pub name_hash: u64,
    pub instructions: Vec<Instruction>,
    pub labels: HashMap<u64, usize>,
}

impl Source {
    pub fn new(name: String, source_file_path: String, instructions: Vec<Instruction>, labels: HashMap<u64, usize>) -> Self {
        Source {
            name: name.clone(),
            source_file_path,
            name_hash: utils::hash_string_vm(&name),
            instructions,
            labels,
        }
    }
}
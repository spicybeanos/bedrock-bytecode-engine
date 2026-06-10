use std::collections::{HashMap, VecDeque};
use crate::vm::VM;
use crate::source::Source;
use crate::vm::GlobalState;

pub struct Engine {
    pub vms: HashMap<u64, VM>,
    pub sources: HashMap<u64, Source>,
    pub global: GlobalState,
    pub scheduler: Scheduler,
}

pub struct Scheduler {
    pub ready_queue: VecDeque<u64>,
    pub instruction_budget: usize,
}

impl Engine {
    pub fn new() -> Self {
        Engine {
            vms: HashMap::new(),
            sources: HashMap::new(),
            global: GlobalState::new(),
            scheduler: Scheduler::new(),
        }
    }
}
impl Scheduler {
    pub fn new() -> Self {
        Scheduler {
            ready_queue: VecDeque::new(),
            instruction_budget: 1000,
        }
    }
}
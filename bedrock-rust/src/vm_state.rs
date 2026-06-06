
pub enum VMState
{
    Running,
    Halted,
    Panicked(u64,String)
}
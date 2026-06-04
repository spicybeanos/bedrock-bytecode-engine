use crate::{
    utils::hash_string_vm,
};


pub fn print_hashes() {
    let fxns = vec![
        "io.console.print",
        "io.console.read_line",
        "io.file.read",
        "io.file.write",
        "io.file.append",
        "io.file.exists",
        "io.file.length",
        "u64.from_string",
        "i64.from_string",
        "f64.from_string",
        "u64.to_string",
        "i64.to_string",
        "f64.to_string",
    ];
    println!("hashes");
    for f in fxns {
        let h = hash_string_vm(f);
        println!("{} -> {} ({:X})",f,h,h);
    }
}

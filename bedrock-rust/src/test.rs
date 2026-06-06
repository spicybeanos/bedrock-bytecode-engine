use crate::{
    utils::hash_string_vm,
};


pub fn print_hashes() {
    let fxns = vec![
        "u64.from_string",
        "i64.from_string",
        "f64.from_string",
        "u64.to_string",
        "i64.to_string",
        "f64.to_string",

        "debug.log",
        "get_delta_time",
        "get_fixed_delta_time",
        "get_time",

        "update_fxns.register_fxn_addr",
        "fixed_update_fxns.register_fxn_addr",
        "start_fxns.register_fxn_addr",

        "create_game_object",
        "get_transform",
        "get_rigidbody",
        "add_force",

        "read_player_input",

    ];
    println!("hashes");
    for f in fxns {
        let h = hash_string_vm(f);
        println!("{} \t->\t {}\t (0x{:X})",f,h,h);
    }
}

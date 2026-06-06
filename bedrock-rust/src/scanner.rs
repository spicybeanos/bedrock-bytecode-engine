use crate::error::BuildError;
use crate::utils::hash_string_vm;
use crate::vm::{ GlobalState, InstrParam, Instruction, VM };
use std::cell::RefCell;
use std::rc::Rc;

enum PreProcesses {
    MemString {
        name: String,
        value: String,
    },
    MemBlob {
        name: String,
        values: String,
        line: usize,
    },
    Label {
        hash: u64,
        line: usize,
    },
}

pub fn construct_from_text(
    id: u64,
    global: Rc<RefCell<GlobalState>>,
    text: &Vec<char>
) -> Result<(VM, Vec<Instruction>), BuildError> {
    let mut vm = VM::new(id, global)?;
    let mut instructions = Vec::<Instruction>::new();
    let mut pre_proccess_cmds = Vec::<PreProcesses>::new();

    let mut index = 0;
    let len = text.len();
    let mut cur = String::new();
    let mut pre_proccess = false;
    let mut line_start = true;
    let mut in_quotes = false;
    let mut line_number = 1usize;
    let mut str_line = Vec::<String>::new();
    let mut str_preproc = Vec::<String>::new();

    while index < len {
        match *text.get(index).ok_or(BuildError::FileSizeLT0.at_line(line_number))? {
            ' ' | '\t' if !in_quotes => {
                push_current_token(&mut cur, pre_proccess, &mut str_line, &mut str_preproc);
                line_start = false;
            }
            '\n' => {
                push_current_token(&mut cur, pre_proccess, &mut str_line, &mut str_preproc);
                finalize_line(
                    pre_proccess,
                    &mut str_line,
                    &mut str_preproc,
                    &mut pre_proccess_cmds,
                    &mut instructions,
                    line_number
                )?;
                pre_proccess = false;
                line_start = true;
                in_quotes = false;
                line_number += 1;
            }
            '\r' => {
                push_current_token(&mut cur, pre_proccess, &mut str_line, &mut str_preproc);
                finalize_line(
                    pre_proccess,
                    &mut str_line,
                    &mut str_preproc,
                    &mut pre_proccess_cmds,
                    &mut instructions,
                    line_number
                )?;
                pre_proccess = false;
                line_start = true;
                in_quotes = false;

                // Treat CRLF as a single line ending.
                if index + 1 < len && text[index + 1] == '\n' {
                    index += 1;
                }
                line_number += 1;
            }
            ':' => {
                if line_start {
                    pre_proccess = true;
                } else {
                    cur.push(':');
                }
                line_start = false;
            }
            '"' => {
                in_quotes = !in_quotes;
                cur.push('"');
                line_start = false;
            }
            c => {
                cur.push(c);
                line_start = false;
            }
        }
        index += 1;
    }

    // Handle any remaining content in the last line.
    push_current_token(&mut cur, pre_proccess, &mut str_line, &mut str_preproc);
    finalize_line(
        pre_proccess,
        &mut str_line,
        &mut str_preproc,
        &mut pre_proccess_cmds,
        &mut instructions,
        line_number
    )?;

    apply_preprocess_cmds(&mut vm, &pre_proccess_cmds)?;

    return Ok((vm, instructions));
}

fn push_current_token(
    cur: &mut String,
    pre_proccess: bool,
    str_line: &mut Vec<String>,
    str_preproc: &mut Vec<String>
) {
    if cur.is_empty() {
        return;
    }

    if pre_proccess {
        str_preproc.push(cur.clone());
    } else {
        str_line.push(cur.clone());
    }
    cur.clear();
}

fn finalize_line(
    pre_proccess: bool,
    str_line: &mut Vec<String>,
    str_preproc: &mut Vec<String>,
    pre_proccess_cmds: &mut Vec<PreProcesses>,
    instructions: &mut Vec<Instruction>,
    line_number: usize
) -> Result<(), BuildError> {
    if pre_proccess {
        if !str_preproc.is_empty() {
            parse_preprocess_line(str_preproc, pre_proccess_cmds, line_number).map_err(|err|
                err.at_line(line_number)
            )?;
        }
        instructions.push(Instruction::Pass);
    } else if !str_line.is_empty() {
        instructions.push(
            instruction_from_string(str_line).map_err(|err| err.at_line(line_number))?
        );
    }

    str_line.clear();
    str_preproc.clear();
    Ok(())
}

fn parse_preprocess_line(
    str_preproc: &Vec<String>,
    pre_proccess_cmds: &mut Vec<PreProcesses>,
    line_number: usize
) -> Result<(), BuildError> {
    let cmd = str_preproc.get(0).ok_or(BuildError::InvalidOrMissingParameter)?.as_str();

    match cmd {
        "string" => {
            let name = str_preproc.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.clone();
            let raw = str_preproc.iter().skip(2).cloned().collect::<Vec<String>>().join(" ");
            if raw.is_empty() {
                return Err(BuildError::InvalidOrMissingParameter);
            }

            let value = raw
                .strip_prefix('"')
                .and_then(|v| v.strip_suffix('"'))
                .unwrap_or(raw.as_str())
                .to_owned();

            pre_proccess_cmds.push(PreProcesses::MemString {
                name,
                value,
            });
            Ok(())
        }
        "buff" => {
            let name = str_preproc.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.clone();
            let values = str_preproc.iter().skip(2).cloned().collect::<Vec<String>>().join(" ");
            if values.is_empty() {
                return Err(BuildError::InvalidOrMissingParameter);
            }

            pre_proccess_cmds.push(PreProcesses::MemBlob {
                name,
                values,
                line: line_number,
            });
            Ok(())
        }
        "label" => {
            let name = str_preproc.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.clone();
            let hash = hash_string_vm(&name);
            pre_proccess_cmds.push(PreProcesses::Label {
                hash,
                line: line_number,
            });
            Ok(())
        }
        _ => Err(BuildError::InvalidParameter),
    }
}

fn apply_preprocess_cmds(
    vm: &mut VM,
    pre_proccess_cmds: &Vec<PreProcesses>
) -> Result<(), BuildError> {
    for cmd in pre_proccess_cmds {
        match cmd {
            PreProcesses::MemString { name, value, .. } => {
                vm.vi_ram.insert(hash_string_vm(name), value.as_bytes().to_vec());
            }
            PreProcesses::MemBlob { name, values, line } => {
                let mut buff = Vec::<u8>::new();
                for token in values.split_whitespace() {
                    buff.push(parse_blob_number(token).map_err(|err| err.at_line(*line))?);
                }
                vm.vi_ram.insert(hash_string_vm(name), buff);
            }
            PreProcesses::Label { hash, line } => {
                vm.label_indexs.insert(*hash, *line);
            }
        }
    }

    Ok(())
}

fn parse_blob_number(token: &str) -> Result<u8, BuildError> {
    let t = token.trim();
    if t.is_empty() {
        return Err(BuildError::InvalidParameter);
    }

    if let Some(hex) = t.strip_prefix("0x").or_else(|| t.strip_prefix("0X")) {
        return u8::from_str_radix(hex, 16).map_err(|_| BuildError::InvalidParameter);
    }

    if t.contains('.') {
        let value: f64 = t.parse::<f64>().map_err(|_| BuildError::InvalidParameter)?;
        if !(0.0..=255.0).contains(&value) {
            return Err(BuildError::InvalidParameter);
        }
        return Ok(value as u8);
    }

    let value: i64 = t.parse::<i64>().map_err(|_| BuildError::InvalidParameter)?;
    if !(0..=255).contains(&value) {
        return Err(BuildError::InvalidParameter);
    }
    Ok(value as u8)
}

fn instruction_from_string(str: &Vec<String>) -> Result<Instruction, BuildError> {
    match str.get(0).ok_or(BuildError::UnkownInstruction)?.as_str() {
        "read" =>
            Ok(
                Instruction::Read(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "write" =>
            Ok(
                Instruction::Write(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "check" =>
            Ok(
                Instruction::Check(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "alloc" =>
            Ok(
                Instruction::Alloc(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "realloc" =>
            Ok(
                Instruction::Realloc(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "free" =>
            Ok(
                Instruction::Free(
                    resolve_param(str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?)?
                )
            ),
        "vlen" =>
            Ok(
                Instruction::Vlen(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "readw" | "oread" =>
            Ok(
                Instruction::ReadW(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "writew" | "owrite" =>
            Ok(
                Instruction::WriteW(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "palloc" =>
            Ok(
                Instruction::Palloc(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "prealloc" =>
            Ok(
                Instruction::Prealloc(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "pfree" =>
            Ok(
                Instruction::Pfree(
                    resolve_param(str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?)?
                )
            ),
        "pcheck" =>
            Ok(
                Instruction::Pcheck(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "pread" =>
            Ok(
                Instruction::Pread(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "pwrite" =>
            Ok(
                Instruction::Pwrite(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "preadw" =>
            Ok(
                Instruction::PreadW(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "pwritew" =>
            Ok(
                Instruction::PwriteW(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "pvlen" =>
            Ok(
                Instruction::Pvlen(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "powner" =>
            Ok(
                Instruction::Powner(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "add" =>
            Ok(
                Instruction::Add(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "sub" =>
            Ok(
                Instruction::Sub(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "mult" =>
            Ok(
                Instruction::Mult(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "div" =>
            Ok(
                Instruction::Div(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "mod" =>
            Ok(
                Instruction::Mod(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "lshift" =>
            Ok(
                Instruction::Lshift(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "rshift" =>
            Ok(
                Instruction::Rshift(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "sqrt" =>
            Ok(
                Instruction::Sqrt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "fadd" =>
            Ok(
                Instruction::Fadd(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fsub" =>
            Ok(
                Instruction::Fsub(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fmult" =>
            Ok(
                Instruction::Fmult(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fdiv" =>
            Ok(
                Instruction::Fdiv(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fmod" =>
            Ok(
                Instruction::Fmod(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fsqrt" =>
            Ok(
                Instruction::Fsqrt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "finv" =>
            Ok(
                Instruction::Finv(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "eq" =>
            Ok(
                Instruction::Eq(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "neq" =>
            Ok(
                Instruction::Neq(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "lt" =>
            Ok(
                Instruction::Lt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "gt" =>
            Ok(
                Instruction::Gt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "lte" =>
            Ok(
                Instruction::Lte(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "gte" =>
            Ok(
                Instruction::Gte(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "and" =>
            Ok(
                Instruction::And(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "or" =>
            Ok(
                Instruction::Or(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "xor" =>
            Ok(
                Instruction::Xor(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "not" =>
            Ok(
                Instruction::Not(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "feq" =>
            Ok(
                Instruction::Feq(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fneq" =>
            Ok(
                Instruction::Fneq(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "flt" =>
            Ok(
                Instruction::Flt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fgt" =>
            Ok(
                Instruction::Fgt(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "flte" =>
            Ok(
                Instruction::Flte(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "fgte" =>
            Ok(
                Instruction::Fgte(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "jmp" =>
            Ok(
                Instruction::Jmp(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?
                    )?
                )
            ),
        "jif" =>
            Ok(
                Instruction::Jif(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_u64_or_hash(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "jnif" =>
            Ok(
                Instruction::Jnif(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_u64_or_hash(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "call" =>
            Ok(
                Instruction::Call(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?
                    )?
                )
            ),
        "ret" => Ok(Instruction::Ret),
        "ecall" =>
            Ok(
                Instruction::Ecall(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_u64_or_hash(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "push" =>
            Ok(
                Instruction::Push(
                    resolve_param(str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?)?
                )
            ),
        "pop" =>
            Ok(
                Instruction::Pop(
                    resolve_param_reg(str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?)?
                )
            ),

        "mov" =>
            Ok(
                Instruction::Mov(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "itof" =>
            Ok(
                Instruction::Itof(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_reg(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "ftoi" =>
            Ok(
                Instruction::Ftoi(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_reg(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "syscallh" =>
            Ok(
                Instruction::SyscallH(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "syscall1" =>
            Ok(
                Instruction::Syscall1(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),
        "" => Ok(Instruction::Pass),

        "halt" => Ok(Instruction::Halt),
        "panic" =>
            Ok(
                Instruction::Panic(
                    resolve_param_u64_or_hash(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        "assert" =>
            Ok(
                Instruction::Assert(
                    resolve_param_reg(
                        str.get(1).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param_u64_or_hash(
                        str.get(2).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?,
                    resolve_param(
                        str.get(3).ok_or(BuildError::InvalidOrMissingParameter)?.as_str()
                    )?
                )
            ),

        _ => Err(BuildError::UnkownInstruction),
    }
}

fn resolve_param_u64_or_hash(param: &str) -> Result<u64, BuildError> {
    let p = param.trim();
    if p.is_empty() {
        return Err(BuildError::InvalidParameter);
    }

    if let Some(sym) = p.strip_prefix(':') {
        return Ok(hash_string_vm(sym));
    }

    if let Some(sym) = p.strip_prefix('.') {
        return Ok(hash_string_vm(sym));
    }

    if let Some(hex) = p.strip_prefix("0x").or_else(|| p.strip_prefix("0X")) {
        return u64::from_str_radix(hex, 16).map_err(|_| BuildError::InvalidParameter);
    }

    p.parse::<u64>().map_err(|_| BuildError::InvalidParameter)
}

fn resolve_param(param: &str) -> Result<InstrParam, BuildError> {
    let p = param.trim();
    if p.is_empty() {
        return Err(BuildError::InvalidParameter);
    }

    if let Some(sym) = p.strip_prefix('.') {
        return Ok(InstrParam::Value(hash_string_vm(sym)));
    }

    if let Some(reg) = p.strip_prefix('r') {
        let reg_id = reg.parse::<u8>().map_err(|_| BuildError::InvalidParameter)?;
        return Ok(InstrParam::Register(reg_id));
    }

    if let Some(hex) = p.strip_prefix("0x").or_else(|| p.strip_prefix("0X")) {
        let value = u64::from_str_radix(hex, 16).map_err(|_| BuildError::InvalidParameter)?;
        return Ok(InstrParam::Value(value));
    }

    if p.contains('.') {
        let value = p.parse::<f64>().map_err(|_| BuildError::InvalidParameter)?;
        return Ok(InstrParam::Value(value.to_bits()));
    }

    // Try parsing as signed integer first (handles negative values)
    if let Ok(value) = p.parse::<i64>() {
        return Ok(InstrParam::Value(value as u64));
    }

    // Fall back to unsigned integer for very large positive values
    let value = p.parse::<u64>().map_err(|_| BuildError::InvalidParameter)?;
    Ok(InstrParam::Value(value))
}

fn resolve_param_reg(param: &str) -> Result<u8, BuildError> {
    let p = param.trim();
    if p.is_empty() {
        return Err(BuildError::InvalidParameter);
    }

    if let Some(reg) = p.strip_prefix('r') {
        let reg_id = reg.parse::<u8>().map_err(|_| BuildError::InvalidParameter)?;
        return Ok(reg_id);
    }

    Err(BuildError::InvalidParameter)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn preprocess_string_inserts_pass_and_memstring() {
        let source = ":string s1 \"hello world\"\nmov r1 .s1\n";
        let text = source.chars().collect::<Vec<char>>();
        let global = GlobalState::new_shared();

        let (vm, instructions) = match construct_from_text(1, global, &text) {
            Ok(v) => v,
            Err(_) => panic!("scanner should parse"),
        };

        assert_eq!(instructions.len(), 2);
        match &instructions[0] {
            Instruction::Pass => {}
            _ => panic!("first instruction should be Pass for preprocessor line"),
        }

        match &instructions[1] {
            Instruction::Mov(reg, InstrParam::Value(v)) => {
                assert_eq!(*reg, 1);
                assert_eq!(*v, hash_string_vm("s1"));
            }
            _ => panic!("second instruction should be mov with hashed symbol"),
        }

        let key = hash_string_vm("s1");
        let bytes = vm.vi_ram.get(&key).expect("preprocessed string should exist");
        assert_eq!(bytes, b"hello world");
    }

    #[test]
    fn preprocess_buff_parses_hex_dec_and_float() {
        let source = ":buff b1 0x41 66 67.9\nmov r2 .b1\n";
        let text = source.chars().collect::<Vec<char>>();
        let global = GlobalState::new_shared();

        let (vm, instructions) = match construct_from_text(2, global, &text) {
            Ok(v) => v,
            Err(_) => panic!("scanner should parse"),
        };

        assert_eq!(instructions.len(), 2);
        match &instructions[0] {
            Instruction::Pass => {}
            _ => panic!("first instruction should be Pass for preprocessor line"),
        }

        match &instructions[1] {
            Instruction::Mov(reg, InstrParam::Value(v)) => {
                assert_eq!(*reg, 2);
                assert_eq!(*v, hash_string_vm("b1"));
            }
            _ => panic!("second instruction should be mov with hashed symbol"),
        }

        let key = hash_string_vm("b1");
        let bytes = vm.vi_ram.get(&key).expect("preprocessed blob should exist");
        assert_eq!(bytes, &vec![0x41, 66, 67]);
    }
}

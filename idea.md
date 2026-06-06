# Bytecode Virtual Machine Intermediate Representation (IR) Specification

## Purpose
This project provides a high-level Intermediate Representation (IR) designed as a compilation target for multiple high-level languages. It provides a secure, sandboxed environment optimized for general-purpose scripting, game modding, and inter-script communication within a common engine.

---

## 1. Core Architecture

### Registers
*   **Capacity:** 256 registers (`r0` through `r255`).
*   **Size:** Each register is fixed at **64-bit**.
*   **Type Handling:** Registers are type-agnostic. Bit-casting allows a single register to store `u64`, `i64`, or `f64` (IEEE 754).

### Memory Model
*   **ViRam (Virtual RAM):** A private, segmented memory space unique to each script instance. It maps virtual handles to real memory blocks. All accesses are bounds-checked.
*   **PVram (Public RAM):** A global shared memory space accessible by all scripts **within the same engine**.
    *   **Ownership:** The script that allocates a block is the "Owner" (identified by a unique `vm_id`).
    *   **Permissions:** Only the Owner can write; all other scripts have read-only access.
    *   **Synchronization:** Uses a **Seqlock** pattern. Readers wait/retry if an Owner is writing to ensure data consistency without heavy mutexes.
*   **Attendance Table:** A global registry where scripts export labels marked as `pub`. This allows Script A to discover and execute functions in Script B securely.

---

## 2. Script Formatting & Compilation
When writing scripts in a text file for the VM to parse or convert to binary, the following rules apply:

*   **Case Sensitivity:** All instructions must be written in **lowercase**.
*   **Registers:** Referenced as `r0` through `r255`.
*   **Constants:**
    *   **Hex:** `0x0000000000000000` - `0xFFFFFFFFFFFFFFFF`.
    *   **Integer:** Standard decimal notation (min/max values of `i64`).
    *   **Float:** Standard floating-point notation (min/max values of `f64`).
*   **Hashing:** If a parameter starts with a dot (`.`), the subsequent string is hashed using **fnv1a_64** to produce a 64-bit integer. (e.g., `.s1` becomes `0x8D8FF07B578D149`).
*   **Whitespace:** Preceding whitespaces and indentation are ignored.
*   **Pre-processor Statements:** Lines starting with `:` are used for static data allocation:
    *   `:string <name> "<content>"`: Allocates the string in ViRam at a handle equal to `hash(name)`.
    *   `:buff <name> <val1> <val2> ...`: Allocates a raw buffer in ViRam at a handle equal to `hash(name)` containing the specified values.

---

## 3. Instruction Set

### Memory & ViRam (Private)
| Instruction | Description |
| :--- | :--- |
| `readw dest src offset` | Reads an 8-byte word from ViRam handle `src` at `offset` into `dest`. |
| `writew src dest offset` | Writes the 8-byte word in `src` to ViRam handle `dest` at `offset`. |
| `read dest src offset` | Reads a 1-byte byte from ViRam handle `src` at `offset` into `dest`. |
| `write src dest offset` | Writes the lowest 1-byte in `src` to ViRam handle `dest` at `offset`. |
| `check dest src offset` | Stores `1` in `dest` if ViRam handle `src` exists and `offset` is in-bounds for an 8-byte access, else `0`. |
| `vlen dest src` | Stores the total size in bytes of local ViRam handle `src` into `dest`. |
| `alloc dest size` | Allocates `size` bytes in local ViRam; stores the virtual handle in `dest`. |
| `realloc dest size` | Resizes local ViRam handle `dest` to `size` bytes (preserving existing data). |
| `free src` | Frees the memory associated with local ViRam handle `src`. |

### PVram (Global Shared)
| Instruction | Description |
| :--- | :--- |
| `preadw dest src offset` | Reads an 8-byte word from Global handle `src` at `offset` (Waits if Owner is writing). |
| `pwritew src dest offset` | Writes an 8-byte word from `src` to Global handle `dest` at `offset` (Owner only; interrupts readers). |
| `pread dest src offset` | Reads a 1-byte byte from Global handle `src` at `offset` (Waits if Owner is writing). |
| `pwrite src dest offset` | Writes lowest 1-byte from `src` to Global handle `dest` at `offset` (Owner only). |
| `pcheck dest src offset` | Stores `1` in `dest` if Global handle `src` exists and `offset` is in-bounds for an 8-byte access. |
| `pvlen dest src` | Stores the total size in bytes of Global handle `src` into `dest`. |
| `palloc dest size` | Allocates `size` bytes in Global PVram (Caller becomes Owner); stores handle in `dest`. |
| `prealloc dest size` | Resizes Global PVram handle `dest` (Owner only). |
| `pfree src` | Frees Global PVram handle `src` (Owner only). |
| `powner dest src` | Writes the Owner ID of PVram address `src` into register `dest`. |

### Arithmetic & Logic (64-bit)
*   **Integer:** `add dest src`, `sub dest src`, `mult dest src`, `div dest src`, `mod dest src`.
*   **Float:** `fadd dest src`, `fsub dest src`, `fmult dest src`, `fdiv dest src`, `fmod dest src`.
*   **Advanced:** `fsqrt dest` (Square root), `finv dest` (Inverse `1.0 / dest`).
*   **Bitwise:** `lshift dest count` (Left shift), `rshift dest count` (Right shift), `and dest src`, `or dest src`, `xor dest src`, `not dest src`.
*   **Comparison (Int):** `eq dest src1 src2`, `neq dest src1 src2`, `lt dest src1 src2`, `gt dest src1 src2`, `lte dest src1 src2`, `gte dest src1 src2` (Stores `1` if true, else `0`).
*   **Comparison (Float):** `feq dest src1 src2`, `fneq dest src1 src2`, `flt dest src1 src2`, `fgt dest src1 src2`, `flte dest src1 src2`, `fgte dest src1 src2`.

### Control Flow & Stack
*   **Labels:** `label .name` (Defines a hashed jump target).
*   **Jumps:** `jmp .name`, `jif src .name` (Jump if `src != 0`), `jnif src .name` (Jump if `src == 0`).
*   **Functions:** `call .name` (Push PC to return-stack and jump), `ret` (Pop from return-stack).
*   **External:** `ecall mod_src label_src` (External Call: Runs `pub` label `label_src` in script `mod_src` via Attendance Table).
*   **Stack:** `push src` (Push register or constant to data stack), `pop dest` (Pop to register).

### Data & System
*   **Conversion:** `itof dest src` (Int to Float), `ftoi dest src` (Float to Int), `mov dest src` (Raw copy).
*   **Inter-op:** `syscallh hash handle dest`, `syscall1 hash arg dest`.

---

## 4. Parameter Specifications
*   **dest:** Destination Register (`r0-r255`).
*   **src / offset / count / size / arg / handle:** Source value. Can be a **Constant** or a **Register**.
*   **count:** Shift limit. Fixed to a maximum value of `64`.
*   **Return Values:** All instructions targeting a `dest` register result in a full 64-bit value storage.

---

## 5. Virtual RAM (ViRam) Implementation Details

The Virtual RAM maps virtual handles to physical memory addresses. This provides:
1.  **Safety:** Direct pointer arithmetic into unauthorized memory is impossible.
2.  **Bounds Enforcement:** Every memory instruction checks the target offset against the allocation size.
3.  **Stability:** `realloc` may move the physical buffer, but the virtual handle remains constant, ensuring script references never break.

## 6. Program execution

These instructions tell the engine what to do with the VM instance
1. **halt**: `halt` retires the from the execution thread. Futher instructions will not be executed and the vm just sits in memory. Instructions from other vms will continue execution.
2. **panic**: `panic <u64 code> <reg/addr/u64 message address>` retires the vm, and stops with an error. This error code and message should be shown to the user, the message address will be a ViRam address
3. **assert**: `assert <rx> <u64 code> <reg/addr/u64 panic message address>` retires the vm if `rx` is 0, and stops with an error. This error message should be shown to the user, the message address will be a ViRam address.

**Example Mapping:**
1. `alloc r1 64`
2. The VM assigns a handle (e.g., `0x100`) to `r1`.
3. ViRam maps `0x100` to a metadata block containing the actual memory address and the size (64 bytes).
4. `readw r2 r1 8` verifies `8 + 8 <= 64` before reading.
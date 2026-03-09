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
*   **PVram (Persistent RAM):** A global shared memory space accessible by all scripts **within the same engine**.
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
    *   `:buff <name> <val1> <val2> ...`: Allocates a raw buffer in ViRam at a handle equal to `hash(name)` containing the specified bytes or values.

---

## 3. Instruction Set

### Memory & ViRam (Private)
| Instruction    | Description                                                                                              |
| :------------- | :------------------------------------------------------------------------------------------------------- |
| `readw x y w`  | Reads an 8-byte word from ViRam handle `y` at byte-offset `w` into register `x`.                         |
| `writew x y w` | Writes the 8-byte word in register `x` to ViRam handle `y` at byte-offset `w`.                           |
| `read x y w`   | Reads a 1-byte byte from ViRam handle `y` at byte-offset `w` into register `x`.                          |
| `write x y w`  | Writes the lowest 1-byte in register `x` to ViRam handle `y` at byte-offset `w`.                         |
| `check x y w`  | Stores `1` in `x` if ViRam handle `y` exists and offset `w` is in-bounds for an 8-byte access, else `0`. |
| `vlen x y`     | Stores the total size in bytes of local ViRam handle `y` into register `x`.                              |
| `alloc x y`    | Allocates `y` bytes in local ViRam; stores the virtual handle in register `x`.                           |
| `realloc x y`  | Resizes local ViRam handle `x` to `y` bytes (preserving existing data).                                  |
| `free x`       | Frees the memory associated with local ViRam handle `x`.                                                 |

### PVram (Global Shared)
| Instruction     | Description                                                                                     |
| :-------------- | :---------------------------------------------------------------------------------------------- |
| `preadw x y w`  | Reads an 8-byte word from Global handle `y` at offset `w` (Waits if Owner is writing).          |
| `pwritew x y w` | Writes an 8-byte word to Global handle `y` at offset `w` (Owner only; interrupts readers).      |
| `pread x y w`   | Reads a 1-byte byte from Global handle `y` at offset `w` (Waits if Owner is writing).           |
| `pwrite x y w`  | Writes lowest 1-byte to Global handle `y` at offset `w` (Owner only).                           |
| `pcheck x y w`  | Stores `1` in `x` if Global handle `y` exists and offset `w` is in-bounds for an 8-byte access. |
| `pvlen x y`     | Stores the total size in bytes of Global handle `y` into register `x`.                          |
| `palloc x y`    | Allocates `y` bytes in Global PVram (Caller becomes Owner); stores handle in `x`.               |
| `prealloc x y`  | Resizes Global PVram handle `x` (Owner only).                                                   |
| `pfree x`       | Frees Global PVram handle `x` (Owner only).                                                     |
| `powner x y`    | Writes the Owner ID of PVram address `y` into register `x`.                                     |

### Arithmetic & Logic (64-bit)
*   **Integer:** `add`, `sub`, `mult`, `div`, `mod`.
*   **Float:** `fadd`, `fsub`, `fmult`, `fdiv`, `fmod`.
*   **Advanced:** `fsqrt x` (Square root), `finv x` (Inverse `1.0/x`).
*   **Bitwise:** `lshift x z` (Left shift), `rshift x z` (Right shift), `and`, `or`, `xor`, `not`.
*   **Comparison (Int):** `eq`, `neq`, `lt`, `gt`, `lte`, `gte` (Stores `1` if true, else `0`).
*   **Comparison (Float):** `feq`, `fneq`, `flt`, `fgt`, `flte`, `fgte`.

### Control Flow & Stack
*   **Labels:** `label .name` (Defines a hashed jump target).
*   **Jumps:** `jmp .name`, `jif x .name` (Jump if `x != 0`), `jnif x .name` (Jump if `x == 0`).
*   **Functions:** `call .name` (Push PC to return-stack and jump), `ret` (Pop from return-stack).
*   **External:** `ecall m h` (External Call: Runs `pub` label `h` in script `m` via Attendance Table).
*   **Stack:** `push rX` (Push register or constant to data stack), `pop rX` (Pop to register).

### Data & System
*   **Conversion:** `itof x y` (Int to Float), `ftoi x y` (Float to Int), `mov x y` (Raw copy).
*   **Inter-op:** `syscallh x y z` (Hashed syscall with ViRam args), `syscall1 x y z` (Hashed syscall with one arg).

---

## 4. Parameter Specifications
*   **x:** Destination Register (`r0-r255`).
*   **y & w:** Source. Can be a **Constant** or a **Register**.
*   **z:** Shift limit. Fixed to a maximum value of `64`.
*   **Return Values:** All instructions targeting `rX` result in a full 64-bit value storage.

---

## 5. Virtual RAM (ViRam) Implementation Details

The Virtual RAM maps virtual handles to physical memory addresses. This provides:
1.  **Safety:** Direct pointer arithmetic into unauthorized memory is impossible.
2.  **Bounds Enforcement:** Every memory instruction checks the target offset against the allocation size.
3.  **Stability:** `realloc` may move the physical buffer, but the virtual handle (e.g., `r1`) remains constant, so the script's references never break.

**Example Mapping:**
1. `alloc r1 64`
2. The VM assigns a handle (e.g., `0x100`) to `r1`.
3. ViRam maps `0x100` to a metadata block containing the actual memory address and the size (64 bytes).
4. `readw r2 r1 8` verifies `8 + 8 <= 64` before reading.
### Purpose
- the purpose of this project is not to make a new language to an already increasing list of languages, but to make a IR code that multiple languages can compile to
- the IR will be fairly high level

### The Unified Instruction Set

**Memory & ViRam (Virtual RAM)**
- `read x y` — Reads the value from ViRam handle `y` (at offset 0) into register `x`.
- `write x y` — Writes the value in register `x` to ViRam handle `y` (at offset 0).
- `oread x y w` — Reads the value from ViRam handle `y` with byte-offset `w` into register `x`.
- `owrite x y w` — Writes the value in register `x` to ViRam handle `y` with byte-offset `w`.
- `check x y w` — Stores `1` in register `x` if ViRam address `y` exists and offset `w` is in bounds, else stores `0`.
- `alloc x y` — Allocates `y` bytes in ViRam and stores the new virtual address handle in `x`.
- `realloc x y` — Resizes ViRam handle `x` to `y` bytes (preserving data).
- `free x` — Frees the memory associated with ViRam handle `x`.

**Integer Arithmetic (64-bit)**
- `add x y` — Stores `x + y` (integer) in register `x`.
- `sub x y` — Stores `x - y` (integer) in register `x`.
- `mult x y` — Stores `x * y` (integer) in register `x`.
- `div x y` — Stores `x / y` (integer) in register `x`.
- `lshift x z` — Stores `x << z` (bitwise left shift) in register `x`.
- `rshift x z` — Stores `x >> z` (bitwise right shift) in register `x`.

**Floating Point Arithmetic (64-bit IEEE 754)**
- `fadd x y` — Stores `x + y` (float) in register `x`.
- `fsub x y` — Stores `x - y` (float) in register `x`.
- `fmult x y` — Stores `x * y` (float) in register `x`.
- `fdiv x y` — Stores `x / y` (float) in register `x`.
- `fsqrt x` — Stores the square root of `x` (float) in register `x`.
- `finv x` — Stores `1.0 / x` (float) in register `x`.

**Data Conversion & Movement**
- `mov x y` — Copies the 64-bit raw value of `y` into register `x`.
- `itof x y` — Converts integer in `y` to float representation and stores in `x`.
- `ftoi x y` — Converts float in `y` to integer representation and stores in `x`.

**Logic & Comparison (Integer)**
- `eq x y z` — Stores `1` in `x` if `y == z`, else `0` (integer compare).
- `neq x y z` — Stores `1` in `x` if `y != z`, else `0` (integer compare).
- `lt x y z` — Stores `1` in `x` if `y < z`, else `0` (integer compare).
- `gt x y z` — Stores `1` in `x` if `y > z`, else `0` (integer compare).
- `lte x y z` — Stores `1` in `x` if `y <= z`, else `0` (integer compare).
- `gte x y z` — Stores `1` in `x` if `y >= z`, else `0` (integer compare).
- `and x y z` — Stores `y & z` (bitwise AND) in `x`.
- `or x y z` — Stores `y | z` (bitwise OR) in `x`.
- `xor x y z` — Stores `y ^ z` (bitwise XOR) in `x`.
- `not x y` — Stores `~y` (bitwise NOT) in `x`.

**Logic & Comparison (Floating Point)**
- `feq x y z` — Stores `1` in `x` if `y == z`, else `0` (float compare).
- `fneq x y z` — Stores `1` in `x` if `y != z`, else `0` (float compare).
- `flt x y z` — Stores `1` in `x` if `y < z`, else `0` (float compare).
- `fgt x y z` — Stores `1` in `x` if `y > z`, else `0` (float compare).
- `flte x y z` — Stores `1` in `x` if `y <= z`, else `0` (float compare).
- `fgte x y z` — Stores `1` in `x` if `y >= z`, else `0` (float compare).

**Control Flow & Stack**
- `label :name` — Defines a jump target (the name is hashed to a `u64` at compile time).
- `jmp :name` — Moves the Program Counter to the instruction following `label :name`.
- `jif x :name` — Jumps to `:name` if register `x` is not zero.
- `jnif x :name` — Jumps to `:name` if register `x` is zero.
- `call :label` — Pushes current PC to return-stack and jumps to `:label`.
- `ret` — Pops value from return-stack and sets PC to that value.
- `push rX` — Pushes value of register `rX` onto the data stack.
- `pop rX` — Pops the top value of the data stack into register `rX`.

**Inter-op & System**
- `syscallh x y z` — Calls engine function (hash `x`) using ViRam argument block `y`; stores result in `z`.
- `syscall1 x y z` — Calls engine function (hash `x`) using single 64-bit argument `y`; stores result in `z`.


**Register sizes are 64-bit**
- Here `x` is a register
- `y`&`w` can be a
	- constant number(`0-9`/`0x0-F`/`0f0-9.0-9`),
	- register(`r1-255`),
- `z`'s value is limited to 64.

Values returned:
- `rX` will give a full 64 bit number.

#### Virtual RAM (ViRam)
The virtual ram is basically a lookup table that maps a virtual address to a real one. You would have noticed that there is a separate read/write instruction to write to an address with an offset in bytes. This is because doing a read/write on an address that is not in the ViRam will result in an error as that address doesn't exists. This also requires that any array venture will first check if it's within bounds or not, thus stopping any buffer overflow attacks.
The ViRam also allows for cleaner reallocs and buffer resizing, as the virtual address(vaddr) does not change even though the actual address has changed after realloc.
This is what happens when you call `alloc r1 64`:
r1 = 0x100 (say)
in the ViRam:
0x100 -> 0x200 (say)
then in memory:
0x200: `[size of buffer(64bits)]`
0x240: `[actual buffer]`
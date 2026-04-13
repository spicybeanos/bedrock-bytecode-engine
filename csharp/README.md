# Bedrock Bytecode Engine - C# Port

This is a C# port of the Bedrock Bytecode Engine, originally written in Rust. It implements a bytecode virtual machine with support for:

## Features

- **256 64-bit Registers**: General-purpose registers for computations
- **Memory Management**: 
  - Local memory (vi_ram) with allocation/reallocation/deallocation
  - Persistent shared memory (p_vram) with ownership tracking
- **Instruction Set**:
  - Integer and floating-point arithmetic operations
  - Memory operations (read/write, word operations)
  - Logic and comparison operations
  - Control flow (jumps, calls, returns, conditional branches)
  - Stack operations (push/pop)
  - System calls for I/O operations (console, file)
- **Error Handling**: Build-time and runtime error reporting
- **Symbol Resolution**: Hash-based label and symbol resolution (FNV-1a hashing)

## Project Structure

- `Error.cs` - Error types and exceptions
- `InstructionTypes.cs` - Instruction definitions and parameter types
- `Utils.cs` - Utility functions (hashing, byte operations)
- `VM.cs` - Virtual machine core implementation
- `Scanner.cs` - Bytecode parser and instruction builder
- `Program.cs` - Entry point with test functions
- `BedrockBytecodeEngine.csproj` - Project file for .NET 8.0

## Building

```bash
dotnet build
```

## Running

To run the default tests:
```bash
dotnet run
```

To execute a .bdt file:
```bash
dotnet run -- path/to/file.bdt
```

## Bytecode Syntax

Instructions follow this pattern:
```
INSTRUCTION reg1 param1 param2
```

Where parameters can be:
- `r0-r255` - Register references
- `123` - Decimal numbers
- `0xFF` - Hexadecimal numbers
- `3.14` - Floating-point numbers
- `.symbol` - Symbol/label references (hashed)

## Preprocessor Directives

- `:string name "value"` - Define a string constant
- `:buff name 0x41 0x42 0x43` - Define a byte buffer
- `:label name` - Define a label for jumps

## System Calls

Supported syscalls via `syscallh`:
- `0x939583C88643DA33` - io.console.print
- `0x9B1285549BD07847` - io.console.read_line
- `0x38E75B427198D6E3` - io.file.read
- `0x5072E3C89F8F6402` - io.file.write
- `0x54351F5BB08D5BF7` - io.file.append

## Key Differences from Rust

- Uses C# exceptions instead of Result types
- Dictionary-based memory instead of Rust's HashMap trait
- Uses .NET string encoding instead of Rust's String
- Class-based instruction definitions instead of Rust enums

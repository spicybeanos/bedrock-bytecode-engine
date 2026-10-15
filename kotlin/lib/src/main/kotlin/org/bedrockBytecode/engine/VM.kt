package org.bedrockBytecode.engine

public class PVramEntry(val OwnerId:ULong, var Data:Array<UByte>)
public const val MaxStackSize = 0x10000
public const val MaxRegisters = 256

public class VM (
    public val id:ULong, 
    public var global:GlobalState = GlobalState.instance,
)
{
    public var reg:Array<ULong> = Array(MaxRegisters,{_ -> 0uL})
    public var pc:Int=0
    public var State:IVMState = Running()
    internal var nextHandle:ULong = 1uL
    public var viRam:HashMap<ULong,ByteArray> = HashMap<ULong,ByteArray>()
    public var labelIndex:HashMap<ULong,Int> = HashMap<ULong,Int>()
    public var stack:ArrayDeque<ULong> = ArrayDeque(MaxStackSize)
    public var callStack:ArrayDeque<Int> = ArrayDeque(MaxStackSize)

}
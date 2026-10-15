package org.bedrockBytecode.engine

import org.bedrockBytecode.engine.Halted

public enum class VMState{
    Running,
    Halted,
    Panicked
}
public interface IVMState{
    public fun getValue() : Int
}
public class Running : IVMState {
    override fun getValue() :Int = -1
}
public class Halted : IVMState {
    override fun getValue(): Int= 0
}

public class Panicked(val ErrorCode:ULong,val ErrorMessage:String) : IVMState {
    override fun getValue(): Int = this.ErrorCode.toUInt().toInt()
}
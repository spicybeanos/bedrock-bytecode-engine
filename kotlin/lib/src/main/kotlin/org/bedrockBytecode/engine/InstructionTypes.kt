package org.bedrockBytecode.engine

internal enum InstrParamType{
    Register,
    Value
}

internal class InstrParam (val Type : InstrParamType , var  RegisterId:UByte,var  Value:ULong){
       

    public fun  Reg( regId:UByte) : InstrParam 
        {
            return InstrParam (Type = InstrParamType.Register, RegisterId = regId,Value = 0UL)
        }

        public fun Val( value:ULong) : InstrParam
        {
            return InstrParam ( Type = InstrParamType.Value, Value = value , RegisterId = 0u)
        }
}

public abstract class Instruction { }



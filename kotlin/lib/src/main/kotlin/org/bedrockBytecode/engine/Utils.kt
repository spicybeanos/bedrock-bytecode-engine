
package org.bedrockBytecode.engine

private const val FNV_OFFSET_BASIS:ULong = 0xcbf29ce484222325uL
private const val FNV_PRIME:ULong = 0x00000100000001B3uL

private fun fnv1a64(data: ByteArray): ULong {
    var hash = FNV_OFFSET_BASIS
    for (b in data) {
        hash = hash xor b.toUByte().toULong()
        hash *= FNV_PRIME
    }
    return hash
}

internal fun hashBlobVm(data:ByteArray) : ULong{
    return fnv1a64(data)
}

internal fun hashStringVm(str:String) : ULong{
    return fnv1a64(str.toByteArray(Charsets.UTF_8))
}

internal fun hashU64Vm(num: ULong): ULong {
    val bytes = ByteArray(8) { i ->
        (num shr (i * 8)).toByte()
    }
    return fnv1a64(bytes)
}
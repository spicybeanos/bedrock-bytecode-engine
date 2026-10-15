
package org.bedrockBytecode.engine

import java.nio.ByteBuffer
import java.nio.ByteOrder


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
private fun buffer(data: ByteArray, offset: Int): ByteBuffer =
    ByteBuffer.wrap(data, offset, data.size - offset)
        .order(ByteOrder.LITTLE_ENDIAN)

internal fun readU64(data: ByteArray, offset: Int): ULong =
    buffer(data, offset).long.toULong()

internal fun readF64(data: ByteArray, offset: Int): Double =
    buffer(data, offset).double

internal fun readI64(data: ByteArray, offset: Int): Long =
    buffer(data, offset).long

internal fun writeU64(data: ByteArray, offset: Int, value: ULong) {
    buffer(data, offset).putLong(value.toLong())
}

internal fun readString(data: ByteArray, offset: Int, len: Int): String =
    String(data, offset, len, Charsets.UTF_8)

internal fun writeString(data: ByteArray, offset: Int, s: String) {
    val bytes = s.toByteArray(Charsets.UTF_8)
    System.arraycopy(bytes, 0, data, offset, bytes.size)
}
package org.bedrockBytecode.engine

class GlobalState {

    internal var nextGlobalHandle: ULong = 1uL

    val pVram: MutableMap<ULong, PVramEntry> = mutableMapOf()

    var vmIds: MutableSet<ULong> = mutableSetOf()

    companion object {
        private var _instance: GlobalState? = null

        @JvmStatic
        var instance: GlobalState
            get() {
                if (_instance == null) {
                    _instance = GlobalState()
                }
                return _instance!!
            }
            set(value) {
                _instance = value
            }

        @JvmStatic
        fun newShared() {
            instance = GlobalState()
        }
    }
}
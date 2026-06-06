using System.Collections.Generic;

namespace BedrockBytecodeEngine
{
    public class GlobalState
    {
        private static GlobalState _instance;
        public static GlobalState Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GlobalState();
                return _instance;
            }
            set
            {
                _instance = value;
            }

        }
        internal ulong nextGlobalHandle = 1;
        public Dictionary<ulong, PVramEntry> PVram { get; } = new Dictionary<ulong, PVramEntry>();
        public HashSet<ulong> vm_ids { get; set; } = new HashSet<ulong>();

        public static void NewShared()
        {
            Instance = new GlobalState();
        }

        public GlobalState()
        {
            
        }
    }
}
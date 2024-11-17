using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Player
{
    public struct PlayerDataStruct : INetworkStruct
    {
        public PlayerRef playerRef;
        public NetworkString<_32> username;
        public Team team;
        public int kill;
        public int death;
    }
}


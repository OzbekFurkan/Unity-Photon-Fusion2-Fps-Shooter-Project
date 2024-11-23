using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

namespace Player
{
    public enum Team { Soldier, Alien }

    [CreateAssetMenu(menuName = "RAW/Interract/Player/PlayerDataSettings")]
    public class PlayerDataSettings : ItemDataSettings
    {
        public Team team;
        public byte HP;
    }

}

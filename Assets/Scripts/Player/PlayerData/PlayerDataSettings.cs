using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

namespace Player
{
    public enum Team { Soldier, Alien }

    [CreateAssetMenu(menuName = "RAW/Data/Player/New Player Data")]
    public class PlayerDataSettings : ItemDataSettings
    {
        public Team team;
        public byte HP;
        public byte startingHP;
    }

}

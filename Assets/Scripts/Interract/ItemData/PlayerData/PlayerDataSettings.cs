using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interract
{
    public enum Team { Blue, Red }

    [CreateAssetMenu(menuName = "RAW/Interract/Player/PlayerDataSettings")]
    public class PlayerDataSettings : ItemDataSettings
    {
        public Team team;
        public byte HP;
    }

}

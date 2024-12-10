using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "RAW/Input/InputSettings")]
    public class InputSettings : ScriptableObject
    {
        [Header("Movement")]
        public KeyCode jumpKey;
        [Header("Weapon")]
        public KeyCode shootKey;
        public KeyCode reloadKey;
        public KeyCode aimKey;
        [Header("Interract")]
        public KeyCode dropkey;
        public KeyCode pickupKey;
        [Header("Item Slot Change")]
        public KeyCode riffleSlotKey;
        public KeyCode pistolSlotKey;
        public KeyCode knifeSlotKey;
        public KeyCode bombSlotKey;
        public KeyCode otherSlotKey;
        [Header("PlayerUI")]
        public KeyCode leaderboardKey;
        public KeyCode pauseKey;

    }
}


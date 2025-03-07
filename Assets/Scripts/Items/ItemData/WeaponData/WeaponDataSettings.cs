using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

namespace Item
{    
    [CreateAssetMenu(menuName = "RAW/Interract/Weapon/WeaponDataSettings")]
    public class WeaponDataSettings : ItemDataSettings
    {
        [Header("Weapon Settings")]
        public int ammo;
        public int fullAmmo;
        public WeaponShootSettings weaponShootSettings;
    }
}

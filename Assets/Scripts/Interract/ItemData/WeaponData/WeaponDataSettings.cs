using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

namespace Interract
{    
    [CreateAssetMenu(menuName = "RAW/Interract/Weapon/WeaponDataSettings")]
    public class WeaponDataSettings : ItemDataSettings
    {
        public int ammo;
        public int fullAmmo;
        public WeaponShootSettings weaponShootSettings;
    }
}

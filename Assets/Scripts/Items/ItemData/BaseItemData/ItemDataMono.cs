using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


namespace Item
{
    public abstract class ItemDataMono : NetworkBehaviour
    {
        //must be assigned in derived classes
        [HideInInspector] public ItemDataSettings itemDataSettings;

        /// <summary>The ItemDataSettings object (holds common attributes datas) should be assigned in this method.
        /// It must be called from Awake, Start, Spawned etc.</summary>
        /// <param name="itemDataSettings">The ItemDataSettings reference of derived object such as WeaponDataSettings, 
        /// PlayerDataSettings etc.</param>
        protected abstract void SetItemDataSettings(ItemDataSettings itemDataSettings);
    }
}
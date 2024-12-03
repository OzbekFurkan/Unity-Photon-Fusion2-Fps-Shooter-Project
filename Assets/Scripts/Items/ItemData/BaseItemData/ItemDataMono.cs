using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


namespace Item
{
    //it is not used yet but might be in future
    public enum ItemState { Available, Interracting, Shooting, Reloading}
    public abstract class ItemDataMono : NetworkBehaviour
    {
        //must be assigned in derived classes
        [HideInInspector] public string itemName;
        [HideInInspector] public int itemId;
        public GameObject itemPrefab;
        [HideInInspector] public Sprite itemIcon;
        [HideInInspector] public int itemSlot;

        /// <summary>
        /// This method is created for initializing the base (ItemDataMono) class's properties,
        /// this method must be called in each derived class when it is first created (start, awake, spawned etc.).
        /// </summary>
        /// <param name="itemId">Unique identifier for each items</param>
        /// <param name="itemPrefab">Prefab gameobject for spawning</param>
        /// <param name="itemIcon">Item icon for inventory demonstration</param>
        /// <param name="itemSlot">Slot data to check if the slot is already taken</param>
        protected abstract void SetBaseProps(string itemName, int itemId, GameObject itemPrefab, Sprite itemIcon, int itemSlot);

    }
}
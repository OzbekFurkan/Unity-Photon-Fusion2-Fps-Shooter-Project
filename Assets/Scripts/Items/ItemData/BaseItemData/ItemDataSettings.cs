using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public enum ItemId { Glock18, P2000, Ak57, M4A4, Famas,Granade, Smoke, Alien, Soldier};
    public enum ItemSlot { Rifle, Pistol, Knife, Bomb, Other, None };
    public class ItemDataSettings : ScriptableObject
    {
        [Header("Common Item Attributes")]
        [Tooltip("If there is no proper option for you, you can add it into itemId enum in ItemDataSettings script")]
        public ItemId itemId;
        [TextArea] public string itemName;
        public GameObject itemPrefab;
        [Tooltip("If the item doesn't have a slot, then choose 'None'")] public ItemSlot itemSlot;
        public Sprite itemIcon;
    }

}

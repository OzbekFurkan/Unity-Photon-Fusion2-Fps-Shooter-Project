using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Item
{
    public enum ItemId { Glock18, P2000, Ak57, M4A4, Famas,Granade, Smoke, Alien, Soldier};
    public enum ItemSlot { Rifle, Pistol, Knife, Bomb, Other, None };
    public class ItemDataSettings : ScriptableObject
    {
        public ItemId itemId;
        [TextArea]
        public string itemName;
        public GameObject itemPrefab;
        public ItemSlot itemSlot;
        public Sprite itemIcon;
    }

}

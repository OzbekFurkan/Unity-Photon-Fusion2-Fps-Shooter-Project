using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interract
{
    public enum ItemId { Glock18, P2000, Ak57, M4A4, Famas,Granade, Smoke, Human, Soldier};
    public enum ItemSlot { Rifle, Pistol, Knife, Bomb, Other, None };
    public class ItemDataSettings : ScriptableObject
    {
        public ItemId itemId;
        public GameObject itemPrefab;
        public ItemSlot itemSlot;
        public Sprite itemIcon;
    }

}

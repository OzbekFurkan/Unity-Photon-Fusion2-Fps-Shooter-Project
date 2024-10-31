using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interract;
using Item;
using Fusion;

namespace InventorySpace
{
    public class Inventory
    {
        public Dictionary<int, object> inventory;

        public Inventory()
        {
            inventory = new Dictionary<int, object>();
        }

        public void AddItem<TDataMono>(TDataMono item) where TDataMono : ItemDataMono
        {
            inventory.Add(item.itemId, item);
        }
        public void DeleteItem(int id)
        {
            inventory.Remove(id);
        }

        /// <summary>
        /// This method can be used to check if the slot is available while picking up an item.
        /// </summary>
        /// <param name="itemData">This object type must be derived from ItemDataMono class and casted into ItemDataMono class</param>
        /// <returns>Returns true if the slot is available</returns>
        public bool SlotEmptyCheck(ItemDataMono itemData, GameObject weaponHolder)
        {
            return !(weaponHolder.transform.GetChild(itemData.itemSlot).childCount>0);
        }

        /// <summary>
        /// Clears all items from inventory.
        /// </summary>
        public void ClearAllItems()
        {
            inventory.Clear();
        }

        /// <summary>
        /// Returns all items that possessed by player
        /// </summary>
        /// <returns></returns>
        public List<GameObject> GetAllItemObjects()
        {
            List<GameObject> items = new List<GameObject>();
            foreach (KeyValuePair<int, object> kvp in inventory)
            {
                ItemDataMono item_inv = (ItemDataMono)kvp.Value;
                items.Add(item_inv.gameObject);
            }
            return items;
        }
        
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player.Interract;
using Item;
using Item.Interract;

namespace Player.Inventory
{
    public class InventoryManager : NetworkBehaviour
    {
        [Header("Player References")]
        [SerializeField] private PlayerInterractManager PIM;
        [SerializeField] private HPHandler hpHandler;
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private Transform weaponHolder;

        [Networked, OnChangedRender(nameof(OnInventoryChanged))]
        public NetworkDictionary<int, NetworkId> inventory { get; } = new NetworkDictionary<int, NetworkId>();

        #region NETWORK_SYNC
        public void OnInventoryChanged()
        {
            DisplayInventory();
        }
        #endregion

        #region INVENTORY_OPERATIONS
        public void AddItem(int itemId , NetworkId networkId)
        {
            inventory.Add(itemId, networkId);
        }
        public void DeleteItem(int itemId)
        {
            inventory.Remove(itemId);
        }

        /// <summary>This method can be used to check if the slot is available while picking up an item.</summary>
        /// <param name="itemData">This object type must be derived from ItemDataMono class and casted into ItemDataMono class</param>
        /// <param name="weaponHolder">weapon holder gameobject that holds all of our items by their slots</param>
        /// <returns>Returns true if the slot is available</returns>
        public bool SlotEmptyCheck(ItemDataMono itemData, GameObject weaponHolder)
        {
            return !(weaponHolder.transform.GetChild((int)itemData.itemDataSettings.itemSlot).childCount > 0);
        }

        /// <summary>Clears all items from inventory.</summary>
        public void ClearAllItems()
        {
            inventory.Clear();
        }

        /// <summary>Returns all items that possessed by player</summary>
        public List<GameObject> GetAllItemObjects()
        {
            List<GameObject> items = new List<GameObject>();
            foreach (KeyValuePair<int, NetworkId> kvp in inventory)
            {
                GameObject item_inv = Runner.FindObject(kvp.Value).gameObject;
                items.Add(item_inv);
            }
            return items;
        }
        #endregion

        #region CALLBACKS
        public void ItemDropped(int itemId)
        {
            Debug.Log("item envanterden çıkarılıyor");
            DeleteItem(itemId);
            DisplayInventory();
        }

        public void ItemPicked(int itemId, NetworkId networkId)
        {
            Debug.Log("item envantere alınıyor");
            AddItem(itemId, networkId);
            DisplayInventory();
        }
        public void DisplayInventory()
        {
            foreach (GameObject item in GetAllItemObjects())
            {
                Debug.Log(GetComponent<PlayerDataMono>().playerData.username+" envanter bilgisi: " + item.name);
            }
        }
        public override void FixedUpdateNetwork()
        {
            if (hpHandler.isDead == false) return;

            if (Object.HasStateAuthority == false) return;

            for (int i = 0; i < weaponHolder.childCount; i++)
            {
                weaponHolder.GetChild(i).gameObject.SetActive(true);

                if (weaponHolder.GetChild(i).childCount <= 0) continue;

                GameObject item = weaponHolder.GetChild(i).GetChild(0).gameObject;
                item.TryGetComponent<InterractComponent>(out var interactComponent);

                if (interactComponent == null) continue;

                interactComponent.isItemActive = true;
                interactComponent.DropItem();

            }

        }
        #endregion
    }
}

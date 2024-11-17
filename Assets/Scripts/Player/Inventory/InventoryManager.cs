using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Reflection;
using Network;
using Utilitiy;
using Interract;
using Player;
using System;
using Item;

namespace InventorySpace
{
    public class InventoryManager : NetworkBehaviour
    {
        [HideInInspector] public Inventory inventory;

        public GameObject StartingWeapon;

        [SerializeField] private PlayerInterractManager PIM;
        [SerializeField] private HPHandler hpHandler;
        [SerializeField] private ItemSwitch itemSwitch;

        bool isPlayerDeath;

        public override void Spawned()
        {
            inventory = new Inventory();
            isPlayerDeath = false;
        }
        public override void FixedUpdateNetwork()
        {
            if(isPlayerDeath)
            {
                List<GameObject> items = inventory.GetAllItemObjects();
                Debug.Log("item sayisi: " + items.Count);
                foreach (GameObject item in items)
                {
                    itemSwitch.SwitchSlot(item.GetComponent<ItemDataMono>().itemSlot);
                    item.GetComponent<InterractComponent>().DropItemRpc();
                }
            }
        }

        private void OnEnable()
        {
            hpHandler.onPlayerDeath += PlayerDied;
            hpHandler.onPlayerRevived += PlayerRevived;
            PIM.onPickUpItem += ItemPicked;
            PIM.onDropItem += ItemDropped;
        }
        private void OnDisable()
        {
            hpHandler.onPlayerDeath -= PlayerDied;
            hpHandler.onPlayerRevived -= PlayerRevived;
            PIM.onPickUpItem -= ItemPicked;
            PIM.onDropItem -= ItemDropped;
        }

        public void ItemDropped(int itemId, dynamic dataMono)
        {
            Debug.Log("item envanterden çıkarılıyor");
            inventory.DeleteItem(itemId);
        }

        public void ItemPicked(int itemId, dynamic dataMono)
        {
            Debug.Log("item envantere alınıyor");
            inventory.AddItem(dataMono);
        }

        public void PlayerDied(PlayerRef killer, GameObject killerGameObject, GameObject deathGO)
        {
            Debug.Log("died notification");
            isPlayerDeath = true;
            
        }
        public void PlayerRevived()
        {
            isPlayerDeath = false;
        }

    }
}

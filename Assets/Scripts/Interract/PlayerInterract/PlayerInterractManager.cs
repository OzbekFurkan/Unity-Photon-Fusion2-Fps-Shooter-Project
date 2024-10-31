using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Network;
using System;
using System.Reflection;
using InventorySpace;
using Item;

namespace Interract
{
    public class PlayerInterractManager : NetworkBehaviour
    {

        List<LagCompensatedHit> detectedInfo;

        LayerMask layerMask;

       [SerializeField] private InventoryManager inventoryManager;
       [SerializeField] private GameObject weaponHolder;


        public delegate void OnPickUpItem(int itemId, dynamic data);
        public event OnPickUpItem onPickUpItem;
        public delegate void OnDropItem(int itemId, dynamic data);
        public event OnDropItem onDropItem;

        public override void Spawned()
        {
            detectedInfo = new List<LagCompensatedHit>();
        }

        public override void FixedUpdateNetwork()
        {

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isDropButtonPressed)
                {
                    HandleDrop();
                }

                if (networkInputData.isPickUpButtonPressed)
                {
                    HandlePickup();
                }

            }

        }

        private void HandleDrop()
        {

            foreach (GameObject item in inventoryManager.inventory.GetAllItemObjects())
            {
                InterractComponent interractComponent = item.GetComponent<InterractComponent>();
                if (interractComponent.isItemActive)
                    interractComponent.DropItemRpc();
            }
        }



        private void HandlePickup()
        {
            layerMask = LayerMask.GetMask("Pickupable");
            Runner.LagCompensation.OverlapSphere(transform.position, 3, Object.InputAuthority, detectedInfo, layerMask, HitOptions.IncludePhysX);
            if (detectedInfo != null)
            {
                Debug.Log(detectedInfo.Count);
                foreach (var info in detectedInfo)
                {
                    if (info.Collider == null)
                        continue;

                    info.Collider.transform.root.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
                    Debug.Log("component alindi " + info.Collider.transform.root.name);
                    if (grabbedItem != null)
                    {
                        Debug.Log("item alindi");
                        info.Collider.transform.root.gameObject.TryGetComponent<ItemDataMono>(out var itemData);
                        Debug.Log("slot uygunluk: " + inventoryManager.inventory.SlotEmptyCheck(itemData, weaponHolder)+": "+itemData.itemSlot);
                        if (itemData!=null && inventoryManager.inventory.SlotEmptyCheck(itemData, weaponHolder))
                        {
                            grabbedItem.PickUpItemRpc(Object.InputAuthority, Object.Id);
                        }  
                        
                        break;
                    }
                    Debug.Log("bos gecti");
                }
            }
        }

        public void SendPickUpCallBack<TDataMono>(NetworkId itemId) where TDataMono:ItemDataMono
        {
            Debug.Log("pickup callback yollandi");
            GameObject itemGO = Runner.FindObject(itemId).gameObject;
            TDataMono dataMono = itemGO.GetComponent<TDataMono>();
            onPickUpItem.Invoke(dataMono.itemId, dataMono);
        }

        public void SendDropCallBack<TDataMono>(NetworkId itemId) where TDataMono : ItemDataMono
        {
            GameObject itemGO = Runner.FindObject(itemId).gameObject;
            TDataMono dataMono = itemGO.GetComponent<TDataMono>();
            onDropItem.Invoke(dataMono.itemId, dataMono);
        }

    }
}


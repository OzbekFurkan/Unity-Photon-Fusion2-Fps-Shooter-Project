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
        [Header("Pickup Circle")]
        List<LagCompensatedHit> detectedInfo;
        LayerMask layerMask;

        [Header("Required Components")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject weaponHolder;
        [SerializeField] private ItemSwitch itemSwitch;

        [Header("Flags")]
        private bool isBusy = false;

        //events
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
            if (isBusy) return;

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isDropButtonPressed)
                {
                    StartCoroutine(HandleDrop());
                }

                if (networkInputData.isPickUpButtonPressed)
                {
                    StartCoroutine(HandlePickup());
                }

            }

        }

        private IEnumerator HandleDrop()
        {
            isBusy = true;
            foreach (GameObject item in inventoryManager.inventory.GetAllItemObjects())
            {
                InterractComponent interractComponent = item.GetComponent<InterractComponent>();
                if (interractComponent.isItemActive)
                {
                    interractComponent.DropItemRpc();
                    yield return new WaitUntil(()=>interractComponent.isDropComplete);
                }
            }
            isBusy = false;
        }



        private IEnumerator HandlePickup()
        {
            isBusy = true;
            layerMask = LayerMask.GetMask("Pickupable");
            Runner.LagCompensation.OverlapSphere(transform.position, 3, Object.InputAuthority, detectedInfo, layerMask, HitOptions.IncludePhysX);
            if (detectedInfo != null && detectedInfo.Count > 0)
            {
                Debug.Log(detectedInfo.Count);
                LagCompensatedHit info = detectedInfo[0];
                for (int i=0; i<detectedInfo.Count-1; i++)
                {
                    info = detectedInfo[i].Distance < detectedInfo[i + 1].Distance ? detectedInfo[i] : detectedInfo[i + 1];
                }
                
                if (info.Collider == null)
                {
                    isBusy = false;
                    yield break;
                }

                info.Collider.transform.root.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
                Debug.Log("component alindi " + info.Collider.transform.root.name);
                if (grabbedItem != null)
                {
                    Debug.Log("item alindi");
                    info.Collider.transform.root.gameObject.TryGetComponent<ItemDataMono>(out var itemData);
                    Debug.Log("slot uygunluk: " + inventoryManager.inventory.SlotEmptyCheck(itemData, weaponHolder) + ": " + itemData.itemSlot);
                    if (itemData != null && inventoryManager.inventory.SlotEmptyCheck(itemData, weaponHolder))
                    {
                        itemSwitch.SwitchSlot(itemData.itemSlot);
                        grabbedItem.PickUpItemRpc(Object.InputAuthority, Object.Id);
                        yield return new WaitUntil(() => grabbedItem.isPickupComplete);
                    }
                }
                Debug.Log("bos gecti");
                
            }
            isBusy = false;
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


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

        public override void Spawned()
        {
            detectedInfo = new List<LagCompensatedHit>();
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
                return;

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
            foreach (GameObject item in inventoryManager.GetAllItemObjects())
            {
                InterractComponent interractComponent = item.GetComponent<InterractComponent>();
                if (interractComponent.isItemActive)
                {
                    interractComponent.DropItemRpc();
                    return;
                }
            }
        }



        private void HandlePickup()
        {
            layerMask = LayerMask.GetMask("Pickupable");
            Runner.LagCompensation.OverlapSphere(transform.position, 3, Object.InputAuthority, detectedInfo, layerMask, HitOptions.IncludePhysX);
            if (detectedInfo != null && detectedInfo.Count > 0)
            {
                Debug.Log(detectedInfo.Count);
                foreach (LagCompensatedHit info in detectedInfo)
                {
                    if (info.Collider == null)
                        continue;

                    info.Collider.transform.root.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
                    Debug.Log("component alindi " + info.Collider.transform.root.name);
                    if (grabbedItem != null)
                    {
                        if (grabbedItem.IsPickedUp)
                            continue;

                        Debug.Log("item alindi");
                        info.Collider.transform.root.gameObject.TryGetComponent<ItemDataMono>(out var itemData);
                        Debug.Log("slot uygunluk: " + inventoryManager.SlotEmptyCheck(itemData, weaponHolder) + ": " + itemData.itemSlot);
                        if (itemData != null && inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
                        {
                            grabbedItem.PickUpItemRpc(Object.InputAuthority, Object.Id);
                            return;
                        }
                    }
                    else
                        continue;

                    Debug.Log("bos gecti");
                }
            }
        }

        public void SendPickUpCallBack(int itemId, NetworkId networkId)
        {
            Debug.Log("pickup callback yollandi");
            inventoryManager.ItemPicked(itemId, networkId);
            
        }

        public void SendDropCallBack(int itemId)
        {
            inventoryManager.ItemDropped(itemId);
        }
       

    }
}


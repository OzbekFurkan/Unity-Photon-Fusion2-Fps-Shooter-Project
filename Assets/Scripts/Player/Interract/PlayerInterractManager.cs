using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player.Inventory;
using Item.Interract;
using Item;
using TMPro;
using UnityEngine.UI;

namespace Player.Interract
{
    public class PlayerInterractManager : NetworkBehaviour
    {
        [Header("Pickup")]
        [Tooltip("The transform of the gameobject that has Camera component"), SerializeField] private Transform cameraTransform;
        LayerMask layerMask;

        [Header("Required Components")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject weaponHolder;
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private PlayerDataMono playerDataMono;

        #region NETWORK_SYNC
        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
                foreach (InterractComponent item in allItems)
                {
                    item.OnRigChange();
                    item.OnColChange();
                }
            }
        }
        #endregion

        #region PICKUP_DROP_CHECK
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
                return;

            CheckPickup();

            //Get the input from the network
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isDropButtonPressed)
                {
                    HandleDrop();
                }
            }

        }
        #endregion

        #region DROP
        private void HandleDrop()
        {
            foreach (GameObject item in inventoryManager.GetAllItemObjects())
            {
                InterractComponent interractComponent = item.GetComponent<InterractComponent>();
                if (interractComponent.isItemActive)
                {
                    playerDataMono.playerState = PlayerState.Interacting;
                    playerDataMono.playerStateStack.Add(playerDataMono.playerState);
                    interractComponent.DropItemRpc();
                    return;
                }
            }
        }
        #endregion

        #region PICKUP
        private void CheckPickup()
        {
            layerMask = LayerMask.GetMask("Pickupable");
            bool isHit = Runner.LagCompensation.Raycast(transform.position, cameraTransform.forward, 3, Object.InputAuthority, out var detectedInfo, layerMask, HitOptions.IncludePhysX);
            if (isHit)
            {
                
                if (detectedInfo.Collider == null)
                    return;

                detectedInfo.Collider.transform.root.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
                Debug.Log("component alindi " + detectedInfo.Collider.transform.root.name);
                if (grabbedItem != null)
                {
                    if (grabbedItem.IsPickedUp)
                        return;
                        

                    detectedInfo.Collider.transform.root.gameObject.TryGetComponent<ItemDataMono>(out var itemData);
                    Debug.Log("slot uygunluk: " + inventoryManager.SlotEmptyCheck(itemData, weaponHolder) + ": " + itemData.itemSlot);
                    if (itemData != null && inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
                    {
                        //display item ui
                        OpenItemUI(grabbedItem, itemData.itemName);

                        //Get the input from the network
                        if (GetInput(out NetworkInputData networkInputData))
                        {
                            if (networkInputData.isPickUpButtonPressed)
                            {
                                HandlePickup(grabbedItem);
                                return;
                            }
                        }

                    }
                    else if(itemData != null && !inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
                    {
                        //display item slot full
                        OpenItemUI(grabbedItem, "Slot Full!");
                        return;
                    }
                    else
                    {
                        //when item null
                        CloseItemUI();
                    }
                }
                else
                {
                    //when item null
                    CloseItemUI();
                }

                Debug.Log("bos gecti");
                
            }
            else
            {
                CloseItemUI();
            }
        }
        private void CloseItemUI()
        {
            InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
            foreach (InterractComponent item in allItems)
            {
                GameObject itemUI = item.GetItemUI();
                if (itemUI != null)
                {
                    itemUI.SetActive(false);
                }
            }
        }
        private void OpenItemUI(InterractComponent grabbedItem, string message)
        {
            GameObject itemUI = grabbedItem.GetItemUI();
            if(itemUI != null)
            {
                itemUI.SetActive(true);
                itemUI.transform.LookAt(transform);
                itemUI.transform.GetChild(0).gameObject.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI itemNameText);
                if(itemNameText != null)
                {
                    itemNameText.text = message;
                }
            }
        }
        private void HandlePickup(InterractComponent grabbedItem)
        {
            playerDataMono.playerState = PlayerState.Interacting;
            playerDataMono.playerStateStack.Add(playerDataMono.playerState);
            grabbedItem.PickUpItemRpc(Object.InputAuthority, Object.Id);
        }
        #endregion

        #region CALLBACKS
        [Rpc(sources:RpcSources.StateAuthority, targets: RpcTargets.InputAuthority|RpcTargets.StateAuthority)]
        public void SendPickUpCallBackRpc(int itemId, NetworkId networkId)
        {
            Debug.Log("pickup callback yollandi");
            inventoryManager.ItemPicked(itemId, networkId);
            playerDataMono.playerStateStack.Remove(PlayerState.Interacting);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
        }
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
        public void SendDropCallBackRpc(int itemId)
        {
            inventoryManager.ItemDropped(itemId);
            playerDataMono.playerStateStack.Remove(PlayerState.Interacting);
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
        }
        #endregion

    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Player.Inventory;
using Item.Interract;
using Item;
using Item.Utils;
using TMPro;
using Fusion.Addons.SimpleKCC;
using UnityEngine.UI;

namespace Player.Interract
{
    public class PlayerInterractManager : NetworkBehaviour
    {
        [Header("Pickup")]
        [Tooltip("The transform of the camera holder gameobject named camera handle"), SerializeField] private Transform cameraHandle;
        LayerMask layerMask;

        [Header("Required References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameObject weaponHolder;
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private PlayerDataMono playerDataMono;
        [SerializeField] private CharacterInputHandler _input;

        #region NETWORK_SYNC
        public override void Spawned()
        {
            //new players should retrieve changes on rigidbody and collider state of the each interactable items in the scene
            if (!Object.HasInputAuthority) return;

            InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
            foreach (InterractComponent item in allItems)
                item.OnPickUpStateChange();
        }
        #endregion

        #region PICKUP_DROP_CHECK
        public override void FixedUpdateNetwork()
        {
            //pick up and drop processes have been done by state authority
            if (!Object.HasStateAuthority) return;

            //getting input from network
            var input = GetInput<NetworkInputData>();
            ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);
            
        }

        private void ProcessInput(NetworkInputData input, NetworkButtons previousButtons)
        {
            CheckPickup(input, previousButtons);

            //input check for dropping item
            if (input.Buttons.WasPressed(previousButtons, InputButton.Drop))
                HandleDrop();
        }
        #endregion

        #region DROP
        private void HandleDrop()
        {
            foreach (GameObject item in inventoryManager.GetAllItemObjects())
            {
                InterractComponent interractComponent = item.GetComponent<InterractComponent>();

                //return if interact component is null
                if (interractComponent == null) continue;

                //do not drop item if it is not our current item
                if (interractComponent.isItemActive == false) continue;

                StartInteractingState();
                interractComponent.DropItem();

                return;//drop only one item at once
            }
        }
        #endregion

        #region PICKUP
        private void CheckPickup(NetworkInputData input, NetworkButtons previousButtons)
        {
            //all pickupable items should be on Pickupable layer
            layerMask = LayerMask.GetMask("Pickupable");
            
            //raycast to detect pickupable items
            bool isHit = Runner.LagCompensation.Raycast(cameraHandle.position, cameraHandle.forward, 3,
                Object.InputAuthority, out var detectedInfo, layerMask, HitOptions.IncludePhysX);

            //close item ui and return when no pickable item is hit
            if (!isHit)
            {
                CloseItemUI_RPC();
                return;
            }
            
            //collider check (just in case)
            if (detectedInfo.Collider == null)  return;
            
            //try to get interact component of hitted item
            detectedInfo.Collider.transform.parent.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
            
            //close item ui and return if there is no interact component
            if (grabbedItem == null)
            {
                CloseItemUI_RPC();
                return;
            }
            
            //return if the item we are trying to pick up is already picked up by someone else
            if (grabbedItem.IsPickedUp) return;
            
            //get the item data component to get the slot info of the item (we will check if it is available)
            detectedInfo.Collider.transform.parent.gameObject.TryGetComponent<ItemDataMono>(out var itemData);

            //close item ui and return if there is no item data component
            if (itemData == null)
            {
                CloseItemUI_RPC();
                return;
            }
            
            //pick up item if the slot is available
            if (inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
            {
                //display item ui
                OpenItemUI_RPC(grabbedItem, itemData.itemDataSettings.itemName, detectedInfo.Point);

                //check input
                if (input.Buttons.WasPressed(previousButtons, InputButton.PickUp))
                {
                    HandlePickup(grabbedItem);
                    return;
                }
            }

            //display slot full warning on item ui if the slot is full
            else if (!inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
                OpenItemUI_RPC(grabbedItem, "Slot Full!", detectedInfo.Point);

        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void CloseItemUI_RPC()
        {
            if (!HasInputAuthority) return;
            
            InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
            foreach (InterractComponent item in allItems)
            {
                ItemReferenceGetter itemReferenceGetter = item.gameObject.GetComponent<ItemReferenceGetter>();

                if (itemReferenceGetter == null) return;

                GameObject itemUI = itemReferenceGetter.ItemUiGetter();

                if (itemUI != null)
                    itemUI.SetActive(false);
            }
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        private void OpenItemUI_RPC(InterractComponent grabbedItem, string message, Vector3 hitPoint)
        {
            if (!HasInputAuthority) return;

            //get item ui using item reference getter component reference
            ItemReferenceGetter itemReferenceGetter = grabbedItem.gameObject.GetComponent<ItemReferenceGetter>();

            if (itemReferenceGetter == null) return;
            
            GameObject itemUI = itemReferenceGetter.ItemUiGetter();

            //item ui null check
            if (itemUI == null) return;
            
            itemUI.SetActive(true);//enable item ui
            itemUI.transform.position = new Vector3(hitPoint.x, cameraHandle.transform.position.y-0.5f, hitPoint.z);
            itemUI.transform.LookAt(cameraHandle);//set its rotation toward our player
            
            //try to get text ui element of grabbed item
            itemUI.transform.GetChild(0).gameObject.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI itemNameText);

            //set the text if the text ui element is found
            if(itemNameText != null)
                itemNameText.text = message;

            
        }
        private void HandlePickup(InterractComponent grabbedItem)
        {
            StartInteractingState();
            grabbedItem.PickUpItem(Object.InputAuthority, Object.Id);
        }
        #endregion

        #region CALLBACKS
        public void SendPickUpCallBack(int itemId, NetworkId networkId)
        {
            Debug.Log("pickup callback sent");
            inventoryManager.ItemPicked(itemId, networkId);
            TerminateInteractingState();
        }

        public void SendDropCallBack(int itemId)
        {
            inventoryManager.ItemDropped(itemId);
            TerminateInteractingState();
        }
        #endregion

        #region STATE_ACTIONS
        private void StartInteractingState()
        {
            //update player state as interacting
            playerDataMono.playerState = PlayerState.Interacting;
            //add current state into player state stack
            playerDataMono.playerStateStack.Add(playerDataMono.playerState);
        }

        private void TerminateInteractingState()
        {
            //remove interact state from stack
            playerDataMono.playerStateStack.Remove(PlayerState.Interacting);
            //get the last state from stack and update player state
            playerDataMono.playerState = playerDataMono.playerStateStack.GetLast();
        }
        #endregion

    }
}


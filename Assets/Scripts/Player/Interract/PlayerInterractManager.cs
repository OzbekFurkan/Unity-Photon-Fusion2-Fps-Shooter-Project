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
            {
                item.OnRigChange();
                item.OnColChange();
            }
        }
        #endregion

        #region PICKUP_DROP_CHECK
        public override void FixedUpdateNetwork()
        {
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
                if (interractComponent == null) return;

                //do not drop item if it is not our current item
                if (interractComponent.isItemActive == false) return;

                StartInteractingState();
                interractComponent.DropItemRpc();
            }
        }
        #endregion

        #region PICKUP
        private void CheckPickup(NetworkInputData input, NetworkButtons previousButtons)
        {
            //all pickupable items should be on Pickupable layer
            layerMask = LayerMask.GetMask("Pickupable");
            Debug.Log("1");
            //raycast to detect pickupable items
            bool isHit = Runner.LagCompensation.Raycast(cameraHandle.position, cameraHandle.forward, 3,
                Object.InputAuthority, out var detectedInfo, layerMask, HitOptions.IncludePhysX);

            //close item ui and return when no pickable item is hit
            if(!isHit)
            {
                CloseItemUI_Wrapper();
                return;
            }
            Debug.Log("2");
            //collider check (just in case)
            if (detectedInfo.Collider == null)  return;
            Debug.Log("3");
            //try to get interact component of hitted item
            detectedInfo.Collider.transform.parent.gameObject.TryGetComponent<InterractComponent>(out var grabbedItem);
            
            //close item ui and return if there is no interact component
            if (grabbedItem == null)
            {
                CloseItemUI_Wrapper();
                return;
            }
            Debug.Log("4");
            //return if the item we are trying to pick up is already picked up by someone else
            if (grabbedItem.IsPickedUp) return;
            Debug.Log("5");
            //get the item data component to get the slot info of the item (we will check if it is available)
            detectedInfo.Collider.transform.parent.gameObject.TryGetComponent<ItemDataMono>(out var itemData);

            //close item ui and return if there is no item data component
            if (itemData == null)
            {
                CloseItemUI_Wrapper();
                return;
            }
            Debug.Log("6");
            //pick up item if the slot is available
            if (inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
            {
                //display item ui
                OpenItemUI_Wrapper(grabbedItem, itemData.itemName);

                //check input
                if (input.Buttons.WasPressed(previousButtons, InputButton.PickUp))
                {
                    HandlePickup(grabbedItem);
                    return;
                }
            }

            //display slot full warning on item ui if the slot is full
            else if (!inventoryManager.SlotEmptyCheck(itemData, weaponHolder))
                OpenItemUI_Wrapper(grabbedItem, "Slot Full!");

        }
        private void CloseItemUI_Wrapper()
        {
            if(Object.HasStateAuthority && Object.HasInputAuthority)
            {
                CloseItemUI();
                return;
            }

            else if(Object.HasStateAuthority)
                CloseItemUI_RPC();

        }
        private void CloseItemUI()
        {       
            InterractComponent[] allItems = GameObject.FindObjectsByType<InterractComponent>(FindObjectsSortMode.None);
            foreach (InterractComponent item in allItems)
            {
                GameObject itemUI = item.GetItemUI();

                if (itemUI != null)
                    itemUI.SetActive(false);
            }
        }
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        private void CloseItemUI_RPC()
        {
            CloseItemUI();
        }
        private void OpenItemUI_Wrapper(InterractComponent grabbedItem, string message)
        {
            if (Object.HasStateAuthority && Object.HasInputAuthority)
            {
                OpenItemUI(grabbedItem, message);
                return;
            }
            else if (Object.HasStateAuthority)
                OpenItemUI_RPC(grabbedItem, message);

        }
        private void OpenItemUI(InterractComponent grabbedItem, string message)
        {
            //get item ui using interact component reference
            GameObject itemUI = grabbedItem.GetItemUI();

            //item ui null check
            if (itemUI == null) return;

            itemUI.SetActive(true);//enable item ui
            itemUI.transform.LookAt(transform);//set its rotation toward our player

            //try to get text ui element of grabbed item
            itemUI.transform.GetChild(0).gameObject.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI itemNameText);

            //set the text if the text ui element is found
            if(itemNameText != null)
                itemNameText.text = message;
                
        }
        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority)]
        private void OpenItemUI_RPC(InterractComponent grabbedItem, string message)
        {
            OpenItemUI(grabbedItem, message);

        }
        private void HandlePickup(InterractComponent grabbedItem)
        {
            StartInteractingState();
            grabbedItem.PickUpItemRpc(Object.InputAuthority, Object.Id);
        }
        #endregion

        #region CALLBACKS
        [Rpc(sources:RpcSources.StateAuthority, targets: RpcTargets.InputAuthority|RpcTargets.StateAuthority)]
        public void SendPickUpCallBackRpc(int itemId, NetworkId networkId)
        {
            Debug.Log("pickup callback sent");
            inventoryManager.ItemPicked(itemId, networkId);
            TerminateInteractingState();
        }

        [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
        public void SendDropCallBackRpc(int itemId)
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


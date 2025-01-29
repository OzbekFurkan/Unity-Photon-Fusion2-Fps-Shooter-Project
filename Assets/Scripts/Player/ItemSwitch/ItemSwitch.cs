using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item;
using Item.Interract;

namespace Player
{

    public class ItemSwitch : NetworkBehaviour
    {
        [Networked, OnChangedRender(nameof(OnSlotChange))] public int currentSlot { get; set; }

        [SerializeField] private Transform weaponHolder;

        public CharacterInputHandler _input;

        #region NETWORK_SYNC
        public void OnSlotChange()
        {
            Debug.Log("slot güncelleniyor");
            for (int i = 0; i < weaponHolder.childCount; i++)
            {
                weaponHolder.GetChild(i).gameObject.SetActive(false);
            }
            weaponHolder.GetChild(currentSlot).gameObject.SetActive(true);
            Debug.Log("slot güncellendi");
        }

        public override void Spawned()
        {
            if(Object.HasInputAuthority)
            {
                ItemSwitch[] allItemSwitches = GameObject.FindObjectsByType<ItemSwitch>(FindObjectsSortMode.None);
                foreach(ItemSwitch itemSwitch in allItemSwitches)
                {
                    itemSwitch.OnSlotChange();
                }
            }
        }
        #endregion

        public override void FixedUpdateNetwork()
        {
            //getting input from network
            var input = GetInput<NetworkInputData>();
            ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);
        }

        private void ProcessInput(NetworkInputData input, NetworkButtons previousButtons)
        {
            // Comparing current input buttons to previous input buttons - this prevents glitches when input is lost
            if (input.Buttons.WasPressed(previousButtons, InputButton.SlotRiffle))
                SwitchSlot((int)ItemSlot.Rifle);
            if (input.Buttons.WasPressed(previousButtons, InputButton.SlotPistol))
                SwitchSlot((int)ItemSlot.Pistol);
            if (input.Buttons.WasPressed(previousButtons, InputButton.SlotKnife))
                SwitchSlot((int)ItemSlot.Knife);
            if (input.Buttons.WasPressed(previousButtons, InputButton.SlotBomb))
                SwitchSlot((int)ItemSlot.Bomb);
            if (input.Buttons.WasPressed(previousButtons, InputButton.SlotOther))
                SwitchSlot((int)ItemSlot.Other);
        }

        public void SwitchSlot(int newSlot)
        {
            for(int i=0; i<weaponHolder.childCount; i++)
            {
                weaponHolder.GetChild(i).gameObject.SetActive(false);
                if(weaponHolder.GetChild(i).childCount>0)
                {
                    GameObject item = weaponHolder.GetChild(i).GetChild(0).gameObject;
                    item.TryGetComponent<InterractComponent>(out var interractComponent);
                    if (interractComponent)
                        interractComponent.isItemActive = false;
                }
               
            }

            weaponHolder.GetChild(newSlot).gameObject.SetActive(true);
            if (weaponHolder.GetChild(newSlot).childCount > 0)
            {
                GameObject item = weaponHolder.GetChild(newSlot).GetChild(0).gameObject;
                item.TryGetComponent<InterractComponent>(out var interractComponent);
                if (interractComponent)
                    interractComponent.isItemActive = true;
            }

            currentSlot = newSlot;
        }




    }

}
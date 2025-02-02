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

        [Header("Player References")]
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
            //disable all slots
            for(int i=0; i<weaponHolder.childCount; i++)
                ToggleSlotEnable(i, false);

            //enable new slot
            ToggleSlotEnable(newSlot, true);

            //set new slot value to networked current slot variable
            currentSlot = newSlot;
        }

        private void ToggleSlotEnable(int slot, bool state)
        {
            weaponHolder.GetChild(slot).gameObject.SetActive(state);
            //if there is an item in that slot, isItemActive value of this item should be updated.
            if (weaponHolder.GetChild(slot).childCount > 0)
            {
                GameObject item = weaponHolder.GetChild(slot).GetChild(0).gameObject;
                item.TryGetComponent<InterractComponent>(out var interractComponent);
                if (interractComponent)
                    interractComponent.isItemActive = state;
            }
        }


    }

}
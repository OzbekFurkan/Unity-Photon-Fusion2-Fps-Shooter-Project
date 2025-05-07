using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Player;
using Player.Utils;
using Item.Interract;

namespace Item
{
    public class WeaponInput : NetworkBehaviour
    {
        GameObject owner = null;
        private IShootable shootManager;
        [SerializeField] private InterractComponent _interract;
        [SerializeField] private WeaponDataMono weaponData;
        [SerializeField] private ItemDataMono itemData;
        private CharacterInputHandler _input;

        [Networked, HideInInspector] public bool isFiring { get; set; } = false;

        private void Awake()
        {
            shootManager = GetComponent<IShootable>();
        }

        public override void FixedUpdateNetwork()
        {

            if (_interract.Owner != null)
                owner = _interract.Owner.gameObject;

            else
                owner = null;

            if (owner != null)
                _input = owner.GetComponent<CharacterInputHandler>();

            //getting input from network
            if(_input != null)
            {
                var input = GetInput<NetworkInputData>();
                ProcessInput(input.GetValueOrDefault(), _input.PreviousButtons);
            }
        }

        private void ProcessInput(NetworkInputData input, NetworkButtons previousButtons)
        {
            if (!_interract.isItemActive)
                return;

            // Update camera pivot so fire transform (CameraHandle) is correct
            SimpleKCC KCC = owner.GetComponent<PlayerReferenceGetter>().GetPlayerKCC();
            Transform CameraPivot = owner.GetComponent<PlayerReferenceGetter>().GetPlayerCameraPivot();
            var pitchRotation = KCC.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            //calculate shoot direction
            Vector3 aimForward = owner.GetComponent<PlayerReferenceGetter>().GetPlayerCameraHandle().transform.forward;

            if ((itemData.itemDataSettings.itemSlot == ItemSlot.Bomb || itemData.itemDataSettings.itemSlot==ItemSlot.Knife) &&
                input.Buttons.WasPressed(previousButtons, InputButton.Fire))
            {
                shootManager.Fire(aimForward);
                return;
            }
            else if (itemData.itemDataSettings.itemSlot == ItemSlot.Bomb || itemData.itemDataSettings.itemSlot == ItemSlot.Knife)
                return;

            //non-auto shoot
            if (!weaponData.weaponShootSettings.isAuto && input.Buttons.WasPressed(previousButtons, InputButton.Fire))
                shootManager.Fire(aimForward);

            //auto shoot input
            else if (weaponData.weaponShootSettings.isAuto && input.Buttons.WasPressed(previousButtons, InputButton.Fire))
                isFiring = true;
            else if (weaponData.weaponShootSettings.isAuto && input.Buttons.WasReleased(previousButtons, InputButton.Fire))
                isFiring = false;

            //auto shooting
            if(isFiring)
                shootManager.Fire(aimForward);
        }

    }
}

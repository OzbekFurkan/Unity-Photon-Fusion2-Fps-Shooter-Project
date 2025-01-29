using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Player;
using Player.UI;
using Item.Interract;

namespace Item
{
    public class WeaponInput : NetworkBehaviour
    {
        GameObject owner = null;
        private ShootManager shootManager;
        private InterractComponent _interract;
        private WeaponDataMono weaponData;
        private CharacterInputHandler _input;

        bool isFiring = false;//for auto mode

        private void Awake()
        {
            shootManager = GetComponent<ShootManager>();
            _interract = GetComponent<InterractComponent>();
            weaponData = GetComponent<WeaponDataMono>();
        }

        public override void FixedUpdateNetwork()
        {

            if (_interract.Owner != null)
                owner = _interract.Owner.gameObject;

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

            // Comparing current input buttons to previous input buttons - this prevents glitches when input is lost
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

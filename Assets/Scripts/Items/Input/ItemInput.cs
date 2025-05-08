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
    public class ItemInput : NetworkBehaviour
    {
        GameObject owner = null;
        private IShootable shootManager;
        [SerializeField] private InterractComponent _interract;
        [SerializeField] private ItemDataMono itemData;
        private CharacterInputHandler _input;

        //auto shoot flag
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

            // Get references
            owner.TryGetComponent<PlayerReferenceGetter>(out PlayerReferenceGetter playerReferenceGetter);
            if (playerReferenceGetter == null) return;

            // Update camera pivot so fire transform (CameraHandle) is correct
            SimpleKCC KCC = playerReferenceGetter.GetPlayerKCC();
            Transform CameraPivot = playerReferenceGetter.GetPlayerCameraPivot();
            var pitchRotation = KCC.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            //calculate shoot direction
            Vector3 aimForward = owner.GetComponent<PlayerReferenceGetter>().GetPlayerCameraHandle().transform.forward;

            SelectProperInput(input, previousButtons, aimForward);
            
        }

        private void SelectProperInput(NetworkInputData input, NetworkButtons previousButtons, Vector3 aimForward)
        {
            TryGetComponent<WeaponDataMono>(out WeaponDataMono weaponDataMono);
            if (weaponDataMono != null)
            {
                bool _isAuto = weaponDataMono.weaponDataSettings.weaponShootSettings.isAuto;
                if (_isAuto)
                    HandleAutoInput(input, previousButtons, aimForward);
                else
                    HandleNonAutoInput(input, previousButtons, aimForward);

                return;
            }

            HandleNonAutoInput(input, previousButtons, aimForward);
            
        }

        private void HandleNonAutoInput(NetworkInputData input, NetworkButtons previousButtons, Vector3 aimForward)
        {
            //non-auto shoot
            if (input.Buttons.WasPressed(previousButtons, InputButton.Fire))
                shootManager.Fire(aimForward);
        }
        private void HandleAutoInput(NetworkInputData input, NetworkButtons previousButtons, Vector3 aimForward)
        {
            //auto shoot input
            if (input.Buttons.WasPressed(previousButtons, InputButton.Fire))
                isFiring = true;
            else if (input.Buttons.WasReleased(previousButtons, InputButton.Fire))
                isFiring = false;

            //auto shooting
            if (isFiring)
                shootManager.Fire(aimForward);
        }

    }
}

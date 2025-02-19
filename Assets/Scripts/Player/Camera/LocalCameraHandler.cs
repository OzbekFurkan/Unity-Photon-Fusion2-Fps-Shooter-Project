using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;

namespace Player
{
    public class LocalCameraHandler : NetworkBehaviour
    {
        [Header("References")]
        public Transform CameraPivot;
        public Transform CameraHandle;
        public Transform WeaponHolder;
        public Transform WeaponHolderTarget;
        public SimpleKCC KCC;
        public CharacterInputHandler _input;

        public override void Render()
        {
            if (HasInputAuthority)
            {
                // Set look rotation for Render.
                KCC.SetLookRotation(_input.lookRotation, -85f, 85f);
            }
        }

        private void LateUpdate()
        {
            // Update camera pivot (influences ChestIK)
            // (KCC look rotation is set earlier in Render)
            var pitchRotation = KCC.GetLookRotation(true, false);
            CameraPivot.localRotation = Quaternion.Euler(pitchRotation);

            // Only InputAuthority needs to update camera
            if (HasInputAuthority)
            {
                // Transfer properties from camera handle to Main Camera.
                Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
            }
            
            // Transfer rotation from weapon holder target to weapon holder.
            WeaponHolder.SetPositionAndRotation(WeaponHolderTarget.position, WeaponHolderTarget.rotation);
        }

    }
}


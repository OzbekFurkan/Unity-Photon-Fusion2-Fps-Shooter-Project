using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.SimpleKCC;
using Item;


namespace Player
{
    [RequireComponent(typeof(SimpleKCC))]
    public class PlayerAnimationHandler : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private SimpleKCC KCC;
        [SerializeField] private Animator animator;

        [Header("Hand Attach")]
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private Transform weaponHolder;
        [Tooltip("Drag animation rig left hand mover's target child object")]
        public Transform leftHandMover;
        [Tooltip("Drag animation rig right hand mover's target child object")]
        public Transform rightHandMover;


        public override void Render()
        {
            SetAnimatorParameters();
            SetHandsOnItem();
        }

        #region SET_ANIMATION_PARAMETERS
        private void SetAnimatorParameters()
        {
            // Convert world velocity to local velocity based on the player's Y-axis rotation
            Vector3 localVelocity = Quaternion.Inverse(KCC.TransformRotation) * KCC.RealVelocity;

            // Apply local velocity to the animator
            animator.SetFloat("vertical", localVelocity.z);
            animator.SetFloat("horizontal", localVelocity.x);
            animator.SetBool("isGrounded", KCC.IsGrounded);
        }
        #endregion

        #region IK_ARM
        private void SetHandsOnItem()
        {
            if (weaponHolder.GetChild(itemSwitch.currentSlot).childCount == 0) return;

            weaponHolder.GetChild(itemSwitch.currentSlot).GetChild(0).TryGetComponent<IHandAttachable>(out var handAttachable);

            if (handAttachable == null)
            {
                //empty hand attachment

                return;
            }

            //non-empty hand attachment

            Transform rightHandTarget = handAttachable.GetRightHandTarget();
            Transform leftHandTarget = handAttachable.GetLeftHandTarget();

            if (rightHandTarget == null || leftHandTarget == null) return;

            PlaceHandOnTarget(rightHandMover, rightHandTarget);

            PlaceHandOnTarget(leftHandMover, leftHandTarget);
        }
        private void PlaceHandOnTarget(Transform handMover, Transform handTarget)
        {
            Vector3 handPos = handTarget.position;
            Quaternion handRot = handTarget.rotation;

            handMover.SetPositionAndRotation(handPos, handRot);
        }
        #endregion

    }

}
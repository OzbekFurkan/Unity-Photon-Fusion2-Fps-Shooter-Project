using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Item;
using Interract;


namespace Player
{
    [RequireComponent(typeof(NetworkCharacterController))]
    public class PlayerAnimationHandler : NetworkBehaviour
    {
        [Header("Components")]
        [SerializeField] private NetworkCharacterController NCC;
        [SerializeField] private Animator animator;

        [Header("Animator Parameters")]
        private float vertical;
        private float horizontal;
        private float isCrouching;
        private bool isGrounded;

        [Header("Hand Attach")]
        [SerializeField] private ItemSwitch itemSwitch;
        [SerializeField] private Transform weaponHolder;
        [Tooltip("Drag Animation Rig left hand target")]
        public Transform leftHandMoverTarget;
        [Tooltip("Drag Animation Rig left hand target")]
        public Transform rightHandMoverTarget;


        public override void FixedUpdateNetwork()
        {
            SetParameterVariables();
            SetAnimatorParameters();
            SetHandsOnItem();
        }

        #region SET_ANIMATION_PARAMETERS
        private void SetParameterVariables()
        {
            isGrounded = NCC.Grounded;
            horizontal = NCC.Velocity.x;
            vertical = NCC.Velocity.y;
        }

        private void SetAnimatorParameters()
        {
            animator.SetFloat("vertical", vertical);
            animator.SetFloat("horizontal", horizontal);
            animator.SetBool("isGrounded", isGrounded);
        }
        #endregion

        #region IK_ARM
        private void SetHandsOnItem()
        {
            if (weaponHolder.GetChild(itemSwitch.currentSlot).childCount == 0)
                return;

            weaponHolder.GetChild(itemSwitch.currentSlot).GetChild(0).TryGetComponent<IHandAttachable>(out var handAttachable);

            if(handAttachable!=null)
            {
                Transform rightHandTarget = handAttachable.GetRightHandTransform();
                Transform leftHandTarget = handAttachable.GetLeftHandTransform();

                SetRightHand(rightHandTarget);

                SetLeftHand(leftHandTarget);

            }
            else
            {
                //empty hand
            }

        }
        private void SetRightHand(Transform rightHandTarget)
        {
            Vector3 rightHandPos = rightHandTarget.position;
            Quaternion rightHandRot = rightHandTarget.rotation;
            rightHandMoverTarget.position = rightHandPos;
            rightHandMoverTarget.rotation = rightHandRot;
        }
        private void SetLeftHand(Transform leftHandTarget)
        {
            Vector3 leftHandPos = leftHandTarget.position;
            Quaternion leftHandRot = leftHandTarget.rotation;
            leftHandMoverTarget.position = leftHandPos;
            leftHandMoverTarget.rotation = leftHandRot;
        }
        #endregion

    }

}
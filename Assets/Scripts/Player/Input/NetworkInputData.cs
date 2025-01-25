using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Player
{
    public struct NetworkInputData : INetworkInput
    {
        //movement
        public Vector2 movementInput;
        public Vector3 aimForwardVector;
        public Vector2 lookRotationVector;
        public NetworkBool isJumpPressed;
        //weapon
        public NetworkBool isFireButtonPressed;
        //interract
        public NetworkBool isDropButtonPressed;
        public NetworkBool isPickUpButtonPressed;
        //item slot switch
        public NetworkBool isRiffleSlotButtonPressed;
        public NetworkBool isPistolSlotButtonPressed;
        public NetworkBool isKnifeSlotButtonPressed;
        public NetworkBool isBombSlotButtonPressed;
        public NetworkBool isOtherSlotButtonPressed;
        //PlayerUI
        public NetworkBool isLeaderboardButtonPressed;
        public NetworkBool isPauseButtonPressed;
    }
}
